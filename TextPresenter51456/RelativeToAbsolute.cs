using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPresenter51456 {
    class RelativeToAbsolute {
        private double absolute;
        private double relative;
        private double reference;

        private void UpdateAbsolute() {
            absolute = relative * reference;
        }

        public double Absolute {
            get { return absolute; }
        }
        public double Relative {
            get { return relative; }
            set {
                relative = value;
                UpdateAbsolute();
            }
        }
        public double Reference {
            get { return reference; }
            set {
                reference = value;
                UpdateAbsolute();
            }
        }

        public static double MakeAbsolute(double relative, double reference) {
            return relative * reference;
        }

        public RelativeToAbsolute(double relative, double reference) {
            this.relative = relative;
            this.reference = reference;
            UpdateAbsolute();
        }
    }
}
