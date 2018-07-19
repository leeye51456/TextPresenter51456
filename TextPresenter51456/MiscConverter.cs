using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TextPresenter51456 {
    class MiscConverter {

        private static FormatException invalidColorEx = new FormatException("Wrong color format");
        private static Regex hexColorRegex = new Regex("(?:#|0x)?([0-9A-Fa-f]{6})");

        // Return relative * reference, relative is 0-1
        public static double RelativeToAbsolute(double relative, double reference) {
            return relative * reference;
        }

        public static SolidColorBrush IntToSolidColorBrush(int hexColor) {
            byte r = (byte)((hexColor / 0x10000) & 0xff);
            byte g = (byte)((hexColor / 0x100) & 0xff);
            byte b = (byte)(hexColor & 0xff);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        public static int StringToIntColor(string hexString) {
            Match regm;
            string tempStr;

            try {
                regm = hexColorRegex.Match(hexString);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw invalidColorEx;
            }

            if (!regm.Success) {
                throw invalidColorEx;
            }

            tempStr = regm.Value;
            return int.Parse(tempStr, System.Globalization.NumberStyles.HexNumber);
        }

    }
}
