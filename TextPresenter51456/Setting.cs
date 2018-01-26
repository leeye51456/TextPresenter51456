using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextPresenter51456 {
    class Setting {
        private static Dictionary<string, string> settings = new Dictionary<string, string>();
        public static UTF8Encoding utf8 = new UTF8Encoding(false);

        public static void SetAttribute(string key, string value) {
            if (settings.ContainsKey(key)) {
                // 기존 -> 변경
                settings[key] = value;
            } else {
                // 신규 -> 추가
                settings.Add(key, value);
            }
        }

        public static string GetAttribute(string key) {
            if (settings.ContainsKey(key))
                return settings[key];
            else
                return null;
        }

        public static void InitializeToDefault() {
            settings.Clear();
            SetAttribute("presenterScreen", System.Windows.Forms.Screen.AllScreens.Length.ToString()); // 마지막 화면
            SetAttribute("marginBasic", "5"); // 기본 여백
            SetAttribute("marginOverflow", "1"); // 넘치는 부분 여백
            SetAttribute("textPosition", "5");
            SetAttribute("textAlign", "2");
            SetAttribute("fontSize", "8.75");
            SetAttribute("lineHeight", "140");
            SetAttribute("resolutionSimulation", "false");
            SetAttribute("resolutionSimulationWidth", "1024");
            SetAttribute("resolutionSimulationHeight", "768");
        }

        private static void InterpretString(string str) {
            try {
                Regex splitter = new Regex(@"^(.*?):(.*)$");
                string key = splitter.Replace(str, "$1");
                string value = splitter.Replace(str, "$2");
                SetAttribute(key, value);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public static bool Load(string fileName = "TextPresenter51456.settings") {
            Regex newLineUnifier = new Regex(@"(\r\n|\r)");
            string fullText;

            InitializeToDefault();

            try {
                StreamReader sr = new StreamReader(fileName, utf8);
                fullText = sr.ReadToEnd();
                sr.Close();
            } catch (Exception exr) {
                Console.WriteLine(exr.Message);
                return Save(fileName);
            }

            fullText = newLineUnifier.Replace(fullText, "\n"); // 줄바꿈 문자 통일
            fullText = fullText.Trim();
            string[] lines = fullText.Split('\n');
            foreach (string item in lines) {
                InterpretString(item);
            }

            return Save(fileName);
        }

        public static bool Save(string fileName = "TextPresenter51456.settings") {
            if (settings.Count == 0) {
                return false;
            }

            try {
                StreamWriter sw = new StreamWriter(fileName, false, utf8);
                try {
                    foreach (KeyValuePair<string, string> item in settings) {
                        sw.WriteLine("{0}:{1}", item.Key, item.Value);
                    }
                } catch (Exception exw) {
                    Console.WriteLine(exw.Message);
                }
                sw.Close();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            return true;
        }

        private Setting() { }
    }
}
