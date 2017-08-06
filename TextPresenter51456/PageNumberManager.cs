using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPresenter51456 {
    class PageNumberManager {
        private bool enabled;
        private int pageNumber;
        private int lastPageNumber;
        private bool allowZero;

        public bool Enabled {
            get { return enabled; }
            set {
                if (value == false) {
                    enabled = false;
                } else if (enabled == false && value == true) {
                    if (allowZero) {
                        pageNumber = 0;
                    } else {
                        pageNumber = 1;
                    }
                }
            }
        }
        public int PageNumber {
            get { return pageNumber; }
            set {
                if (enabled && ((value > 0 && value <= lastPageNumber) || (allowZero && value == 0))) {
                    pageNumber = value;
                }
            }
        }

        public int LastPageNumber {
            get { return lastPageNumber; }
            set {
                if (enabled && (value > 0 || (allowZero && value == 0))) {
                    lastPageNumber = value;
                    if (lastPageNumber < pageNumber) {
                        pageNumber = lastPageNumber;
                    }
                }
            }
        }

        public bool AllowZero {
            get { return allowZero; }
            set {
                if (enabled) {
                    if (value == false && (pageNumber == 0 || lastPageNumber == 0)) {
                        pageNumber = 1;
                        lastPageNumber = 1;
                    }
                    allowZero = value;
                }
            }
        }

        public PageNumberManager() {
            enabled = true;
            allowZero = false;
            pageNumber = 1;
            lastPageNumber = 1;
        }
        public PageNumberManager(int initialPageNumber, int lastPageNumber, bool allowZero) {
            enabled = true;
            if (initialPageNumber > lastPageNumber) {
                // 꼬인 경우
                this.allowZero = allowZero;
                pageNumber = 1;
                this.lastPageNumber = 1;
            } else if (initialPageNumber > 0) {
                this.allowZero = allowZero;
                pageNumber = initialPageNumber;
                this.lastPageNumber = lastPageNumber;
            } else if (initialPageNumber == 0) {
                this.allowZero = true;
                pageNumber = initialPageNumber;
                this.lastPageNumber = lastPageNumber;
            } else {
                // 잘못된 경우
                this.allowZero = allowZero;
                pageNumber = 1;
                this.lastPageNumber = 1;
            }
        }
    }
}
