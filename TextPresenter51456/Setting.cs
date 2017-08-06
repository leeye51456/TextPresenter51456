using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TextPresenter51456 {
    class Setting {
        private static Dictionary<string, string> settings = new Dictionary<string, string>();

        public static void InitializeToDefault() {
            settings.Clear();
            settings.Add("usePvw", "true");
            settings.Add("useMouse", "true");
            settings.Add("useKeyboard", "true");
            settings.Add("updateOnChange", "false");
            settings.Add("presenterScreen", "2");
            settings.Add("textPosition", "5");
            settings.Add("textAlign", "2");
            settings.Add("fontFamily", "NanumBarunGothic");
            settings.Add("fontSize", "8.75");
            settings.Add("lineHeight", "140");
        }

        public static void Set(string key, string value) {
            if (settings.ContainsKey(key))
                settings[key] = value;
            else
                settings.Add(key, value);
        }

        public static string Get(string key) {
            if (settings.ContainsKey(key))
                return settings[key];
            else
                return null;
        }

        public static bool Load(string fileName = "settings.xml") {
            InitializeToDefault();
            XmlDocument xdoc = new XmlDocument();

            try {
                xdoc.Load(fileName);
            } catch (Exception e) {
                return Save(fileName);
            }

            try {
                XmlNodeList nodes = xdoc.SelectNodes("/settings");

                foreach (XmlNode node in nodes) {
                    Set(node.Name, node.InnerText);
                }
            } catch (Exception e) {
                return false;
            }

            return true;
        }

        public static bool Save(string fileName = "settings.xml") {
            if (settings.Count == 0) {
                return false;
            }
            try {
                XmlDocument xdoc = new XmlDocument();
                XmlNode root = xdoc.CreateElement("settings");
                xdoc.AppendChild(root);

                Dictionary<string, XmlNode> xnode = new Dictionary<string, XmlNode>();

                foreach (string key in settings.Keys) {
                    xnode.Add(key, xdoc.CreateElement(key));
                    xnode[key].InnerText = settings[key];
                    root.AppendChild(xnode[key]);
                }

                xdoc.Save(fileName);
            } catch (Exception e) {
                return false;
            }

            return true;
        }

        private Setting() { }
    }
}
