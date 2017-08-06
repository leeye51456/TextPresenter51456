using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPresenter51456 {
    class PageNumberLog {
        private string log;
        private bool enabled;
        private bool useKeypad;


        public string Log {
            get { return log; }
        }

        public bool Enabled {
            get { return enabled; }
            set {
                if (enabled == true && value == false) {
                    ClearLog();
                }
                enabled = value;
            }
        }

        public bool UseKeypad {
            get { return useKeypad; }
            set { useKeypad = value; }
        }


        private void CutLog() {
            if (enabled) {
                int intLog;
                if (log.Length > 5) {
                    log = log.Substring(log.Length - 5);
                }
                if (!Int32.TryParse(log, out intLog)) {
                    return;
                }
                log = intLog.ToString();
            }
        }

        public void Add(int digit) {
            if (enabled) {
                log = log + digit.ToString();
                CutLog();
            }
        }
        public void Add(string digits) {
            if (enabled) {
                log = log + digits;
                CutLog();
            }
        }

        public void ClearLog() {
            if (enabled) {
                log = "";
            }
        }

        public int getPageNumber() {
            if (log.Equals("")) {
                return -1;
            }
            return Int32.Parse(log);
        }

        public PageNumberLog() {
            enabled = true;
            useKeypad = true;
            log = "";
        }
    }
}
