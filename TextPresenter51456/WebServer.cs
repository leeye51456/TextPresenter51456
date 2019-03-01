using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextPresenter51456 {

    public class StateObject {
        public Socket workSocket = null;
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[bufferSize];
        public StringBuilder sb = new StringBuilder();
    }


    class WebServer {
        public static MainWindow mw;

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static int ServerPort { get; set; } = 51456;
        private static string[] Crlf { get; } = { "\r\n" };
        private static TimeSpan longPollingTimeout = new TimeSpan(0, 0, 30);


        public static void StartListening() {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress serverIpAddress = ipHostInfo.AddressList[0];

            IPEndPoint localEndPoint = new IPEndPoint(serverIpAddress, ServerPort);

            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try {
                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(int.MaxValue);

                while (true) {
                    allDone.Reset();
                    listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
                    allDone.WaitOne();
                }
            } catch (Exception) {
                // something wrong
            } finally {
                listenerSocket.Close();
            }
        }

        private static void AcceptCallback(IAsyncResult ar) {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject {
                workSocket = handler
            };
            handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar) {
            string request = string.Empty;
            string[] requestHeaders;
            string[] startLine;
            string requestBody;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0) {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                request = state.sb.ToString();
                if (request.EndsWith("\r\n")) {
                    int indexOfCrlf2 = request.IndexOf("\r\n\r\n");
                    Console.WriteLine("\n [Message from client]\n{0}", request);

                    if (indexOfCrlf2 == -1) { // no additional data
                        requestHeaders = request.Substring(0, request.Length - 2).Split(Crlf, StringSplitOptions.None);
                        requestBody = string.Empty;
                    } else {
                        requestHeaders = request.Substring(0, indexOfCrlf2).Split(Crlf, StringSplitOptions.None);
                        requestBody = request.Substring(indexOfCrlf2 + 4, request.Length - indexOfCrlf2 - 6);
                    }

                    startLine = requestHeaders[0].Split();

                    Send(handler, MakeResponse(startLine[0].Trim(), startLine[1].Trim(), requestBody));
                } else {
                    handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }
        private static string MakeResponse(string method, string fileName, string requestBody) {
            string response = "HTTP/1.1 200 OK\r\n";
            string responseBody = null;

            if (fileName.EndsWith(".html") || !fileName.Contains(".")) {
                response += "Content-Type: text/html; charset=utf-8\r\n";
            } else if (fileName.EndsWith(".js")) {
                response += "Content-Type: text/javascript; charset=utf-8\r\n";
            } else if (fileName.EndsWith(".css")) {
                response += "Content-Type: text/css; charset=utf-8\r\n";
            } else if (fileName.EndsWith(".json")) {
                response += "Content-Type: application/json; charset=utf-8\r\n";
                if (method.Equals("POST")) {
                    // get session id/state
                    string[] requestParams = requestBody.Split(',');
                    if (!long.TryParse(requestParams[0].Trim(), out long sid)) {
                        sid = -1;
                    }
                    if (!long.TryParse(requestParams[1].Trim(), out long sstate)) {
                        sstate = -1;
                    }

                    DateTime dtBegin = DateTime.Now;
                    while (DateTime.Now - dtBegin < longPollingTimeout) {
                        // session id/state changed
                        if (sid != Session.Id || sstate != Session.State) {
                            responseBody = string.Format(
                                "{{\"sid\":{0},\"state\":{1},\"pgm\":{2},\"pvw\":{3},\"next\":{4}}}",
                                Session.Id, Session.State, Session.Pgm, Session.Pvw, Session.Next);
                        }
                    }
                    // timeout: no change
                    if (responseBody == null) {
                        responseBody = "{\"sid\":0}";
                    }
                }
                /*
            } else if (fileName.Equals("favicon.ico")) {
                response += "Content-Type: image/png; charset=utf-8\r\n";
                // ...
                */
            } else {
                response += "Content-Type: text/plain; charset=utf-8\r\n";
            }
            response += "Cache-Control: no-cache\r\n";
            response += "\r\n";

            if (responseBody == null) {
                try {
                    StreamReader sr = new StreamReader(fileName, Encoding.UTF8); // Encoding.ASCII ?
                    response += sr.ReadToEnd();
                    sr.Close();
                } catch (Exception) {
                    return "HTTP/1.1 404 Not Found\r\n";
                }
            } else {
                response += responseBody;
            }
            response += "\r\n";

            return response;
        }

        private static void Send(Socket handler, string data) {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
