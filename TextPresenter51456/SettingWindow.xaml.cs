using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TextPresenter51456 {
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window {
        Window mw, pw;

        int presenterScreen, textPosition, textAlign;
        decimal marginBasic, marginOverflow, fontSize, lineHeight;

        private void GetSettings() {
            if (!int.TryParse(Setting.GetAttribute("presenterScreen"), out presenterScreen)) {
                presenterScreen = System.Windows.Forms.Screen.AllScreens.Length;
            }
            if (!decimal.TryParse(Setting.GetAttribute("marginBasic"), out marginBasic)) {
                marginBasic = 5;
            }
            if (!decimal.TryParse(Setting.GetAttribute("marginOverflow"), out marginOverflow)) {
                marginOverflow = 1;
            }
            if (!int.TryParse(Setting.GetAttribute("textPosition"), out textPosition)) {
                textPosition = 5;
            }
            if (!int.TryParse(Setting.GetAttribute("textAlign"), out textAlign)) {
                textAlign = 2;
            }
            if (!decimal.TryParse(Setting.GetAttribute("fontSize"), out fontSize)) {
                fontSize = 8.75m;
            }
            if (!decimal.TryParse(Setting.GetAttribute("lineHeight"), out lineHeight)) {
                lineHeight = 140;
            }
        }
        private void SettingToControl() {
            GetSettings();
            if (presenterScreen <= ComboBoxPresenterScreen.Items.Count) {
                ComboBoxPresenterScreen.SelectedIndex = presenterScreen - 1;
            } else {
                ComboBoxPresenterScreen.SelectedIndex = System.Windows.Forms.Screen.AllScreens.Length - 1;
            }
            /*
            switch (textPosition) {
            case 1:
                RadioButtonTextPosition1.IsChecked = true;
                break;
            case 2:
                RadioButtonTextPosition2.IsChecked = true;
                break;
            case 3:
                RadioButtonTextPosition3.IsChecked = true;
                break;
            case 4:
                RadioButtonTextPosition4.IsChecked = true;
                break;
            case 5:
                RadioButtonTextPosition5.IsChecked = true;
                break;
            case 6:
                RadioButtonTextPosition6.IsChecked = true;
                break;
            case 7:
                RadioButtonTextPosition7.IsChecked = true;
                break;
            case 8:
                RadioButtonTextPosition8.IsChecked = true;
                break;
            case 9:
                RadioButtonTextPosition9.IsChecked = true;
                break;
            }
            ComboBoxTextAlign.SelectedIndex = textAlign - 1;
            */
            TextBoxFontSize.Text = fontSize.ToString();
            TextBoxLineHeight.Text = lineHeight.ToString();
        }

        public SettingWindow(Window mw, Window presenter) {
            this.mw = mw;
            pw = presenter;

            InitializeComponent();

            SettingToControl();
        }

        private void TextBoxDouble_KeyDown(object sender, KeyEventArgs e) {
            if (!(e.Key == Key.Back || (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal)) {
                // 숫자, 백스페이스, 점만 처리
                e.Handled = true;
            }
        }

        private int GetCheckedTextPosition() {
            if ((bool)RadioButtonTextPosition1.IsChecked)
                return 1;
            else if ((bool)RadioButtonTextPosition2.IsChecked)
                return 2;
            else if ((bool)RadioButtonTextPosition3.IsChecked)
                return 3;
            else if ((bool)RadioButtonTextPosition4.IsChecked)
                return 4;
            else if ((bool)RadioButtonTextPosition5.IsChecked)
                return 5;
            else if ((bool)RadioButtonTextPosition6.IsChecked)
                return 6;
            else if ((bool)RadioButtonTextPosition7.IsChecked)
                return 7;
            else if ((bool)RadioButtonTextPosition8.IsChecked)
                return 8;
            else if ((bool)RadioButtonTextPosition9.IsChecked)
                return 9;
            else
                return 0;
        }
        private void ButtonApply_Click(object sender, RoutedEventArgs e) {
            Setting.SetAttribute("presenterScreen", (ComboBoxPresenterScreen.SelectedIndex + 1).ToString());
            Setting.SetAttribute("marginBasic", "5"); ////////////////
            Setting.SetAttribute("marginOverflow", "1"); ///////////////
            Setting.SetAttribute("textPosition", GetCheckedTextPosition().ToString());
            Setting.SetAttribute("textAlign", ComboBoxTextAlign.Text.ToString());
            Setting.SetAttribute("fontSize", TextBoxFontSize.Text.ToString());
            Setting.SetAttribute("lineHeight", TextBoxLineHeight.Text.ToString());
            Setting.Save();

            if (pw != null) {
                RoutedEventArgs rea = new RoutedEventArgs(LoadedEvent);
                rea.Source = pw;
                pw.RaiseEvent(rea);
            }

            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
