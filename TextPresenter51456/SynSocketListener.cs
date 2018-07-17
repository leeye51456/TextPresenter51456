using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextPresenter51456 {
    public class SynSocketListener {
        public static MainWindow mw;


        private static int Port { get; set; } = 51456; // test port

        private static Socket handler;

        // $1: command, $2: parameter
        private static Regex commandCutter = new Regex(@"^(\w+):([\s\S]*)<EndOfCommand>[\s\S]*");

        private static string cmd = null;
        private static string param = null;
        private static string data = null;


        private static string ProcessMessage() {
            return mw.PreProcessRemote(cmd, param);
        }


        public static void StartListening() {
            if (mw == null) {
                Console.WriteLine("SynSocketListener 초기화 안 됨");
                return;
            }

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            IPEndPoint localEndPoint = new IPEndPoint(0, Port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.ReceiveTimeout = 2000;
            listener.SendTimeout = 2000;

            try {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true) {
                    Console.WriteLine("연결 기다리는 중...");
                    handler = listener.Accept();

                    data = null;
                    cmd = null;
                    param = null;

                    while (true) {
                        bytes = new byte[1024];
                        int bytesRecv = handler.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRecv);
                        if (data.IndexOf("<EndOfCommand>") > -1) {
                            break;
                        }
                    }

                    data = data.Trim();
                    cmd = commandCutter.Replace(data, "$1");
                    param = commandCutter.Replace(data, "$2");
                    Console.WriteLine("받은 문자열: {0}", data);

                    byte[] msg = Encoding.UTF8.GetBytes(ProcessMessage());
                    handler.Send(msg);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    handler = null;

                    if (cmd.Equals("terminate")) {
                        break;
                    }
                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            listener.Close();

            Console.WriteLine("\n원격 제어 종료됨");

        }


        public static void TerminateListening() {
            // send terminate message to itself
            try {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, 51456);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try {
                    sender.Connect(remoteEP);

                    byte[] msg = Encoding.UTF8.GetBytes("terminate:<EndOfCommand>");
                    sender.Send(msg);

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                } catch (ArgumentNullException ane) {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                } catch (SocketException se) {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                } catch (Exception e) {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
