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
        MainWindow mw;
        PresenterWindow pw;

        int presenterScreen, textPosition, textAlign, resolutionSimulationWidth, resolutionSimulationHeight;
        decimal marginBasic, marginOverflow, fontSize, lineHeight;
        bool resolutionSimulation;

        private void GetSettings() {
            if (!int.TryParse(Setting.GetAttribute("presenterScreen"), out presenterScreen)) {
                presenterScreen = System.Windows.Forms.Screen.AllScreens.Length;
            }
            if (!decimal.TryParse(Setting.GetAttribute("marginBasic"), out marginBasic) || marginBasic < 0) {
                marginBasic = 5;
            }
            if (!decimal.TryParse(Setting.GetAttribute("marginOverflow"), out marginOverflow) || marginOverflow < 0) {
                marginOverflow = 1;
            }
            if (!int.TryParse(Setting.GetAttribute("textPosition"), out textPosition) || textPosition < 1 || textPosition > 9) {
                textPosition = 5;
            }
            if (!int.TryParse(Setting.GetAttribute("textAlign"), out textAlign) || textAlign < 1 || textAlign > 3) {
                textAlign = 2;
            }
            if (!decimal.TryParse(Setting.GetAttribute("fontSize"), out fontSize) || fontSize <= 0) {
                fontSize = 8.75m;
            }
            if (!decimal.TryParse(Setting.GetAttribute("lineHeight"), out lineHeight) || lineHeight < 0) {
                lineHeight = 140;
            }
            if (!bool.TryParse(Setting.GetAttribute("resolutionSimulation"), out resolutionSimulation)) {
                resolutionSimulation = false;
            }
            if (!int.TryParse(Setting.GetAttribute("resolutionSimulationWidth"), out resolutionSimulationWidth) || resolutionSimulationWidth <= 0) {
                resolutionSimulationWidth = 1024;
            }
            if (!int.TryParse(Setting.GetAttribute("resolutionSimulationHeight"), out resolutionSimulationHeight) || resolutionSimulationHeight <= 0) {
                resolutionSimulationHeight = 768;
            }
        }

        private void SettingToControl() {
            System.Windows.Forms.Screen[] sc = System.Windows.Forms.Screen.AllScreens;
            int numOfScreen = sc.Length;

            GetSettings();

            // 임시조치 시작: 화면 배치 미리보기 미구현
            CanvasScreens.Visibility = Visibility.Hidden;
            CanvasScreens.Height = 0;
            // 임시조치 끝

            ComboBoxPresenterScreen.Items.Clear();
            for (int i = 0; i < numOfScreen; i++) {
                System.Drawing.Rectangle r = sc[i].Bounds;
                ComboBoxItem cbi = new ComboBoxItem();
                // (2) 1024x768 (1920,0)
                cbi.Content = "(" + (i + 1).ToString() + ") " + r.Width.ToString() + "×" + r.Height.ToString() + " (" + r.X.ToString() + "," + r.Y.ToString() + ")";
                ComboBoxPresenterScreen.Items.Add(cbi);
            }
            if (presenterScreen <= numOfScreen) {
                ComboBoxPresenterScreen.SelectedIndex = presenterScreen - 1;
            } else {
                ComboBoxPresenterScreen.SelectedIndex = numOfScreen - 1;
            }

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

            CheckBoxResolutionSimulation.IsChecked = resolutionSimulation;
            TextBoxScreenWidth.Text = resolutionSimulationWidth.ToString();
            TextBoxScreenHeight.Text = resolutionSimulationHeight.ToString();
            ComboBoxTextAlign.SelectedIndex = textAlign - 1;
            TextBoxFontSize.Text = fontSize.ToString();
            TextBoxLineHeight.Text = lineHeight.ToString();
            TextBoxMarginBasic.Text = marginBasic.ToString();
            TextBoxMarginOverflow.Text = marginOverflow.ToString();
        }

        public SettingWindow(MainWindow mw, PresenterWindow pw) {
            this.mw = mw;
            this.pw = pw;

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
        private void ShowWrongFormatMessage(string name, string format) {
            MessageBox.Show("'" + name + "'의 형식이 잘못되었습니다.\n" + format + "만 입력 가능합니다.",
                "TextPresenter51456",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        private bool Apply() {
            decimal tempd;
            if (!int.TryParse(Setting.GetAttribute("resolutionSimulationWidth"), out resolutionSimulationWidth) || resolutionSimulationWidth <= 0) {
                ShowWrongFormatMessage("해상도 시뮬레이션 가로 길이", "양의 정수");
                return false;
            }
            if (!int.TryParse(Setting.GetAttribute("resolutionSimulationHeight"), out resolutionSimulationHeight) || resolutionSimulationHeight <= 0) {
                ShowWrongFormatMessage("해상도 시뮬레이션 세로 길이", "양의 정수");
                return false;
            }
            if (!decimal.TryParse(TextBoxFontSize.Text, out tempd) || tempd <= 0) {
                ShowWrongFormatMessage("텍스트 크기", "양수");
                return false;
            }
            if (!decimal.TryParse(TextBoxLineHeight.Text, out tempd) || tempd <= 0) {
                ShowWrongFormatMessage("줄 간격", "양수");
                return false;
            }
            if (!decimal.TryParse(TextBoxMarginBasic.Text, out tempd) || tempd < 0) {
                ShowWrongFormatMessage("기본 여백", "0 또는 양수");
                return false;
            }
            if (!decimal.TryParse(TextBoxMarginOverflow.Text, out tempd) || tempd < 0) {
                ShowWrongFormatMessage("넘치는 부분 여백", "0 또는 양수");
                return false;
            }

            Setting.SetAttribute("presenterScreen", (ComboBoxPresenterScreen.SelectedIndex + 1).ToString());
            Setting.SetAttribute("resolutionSimulation", CheckBoxResolutionSimulation.IsChecked.ToString());
            Setting.SetAttribute("resolutionSimulationWidth", TextBoxScreenWidth.Text);
            Setting.SetAttribute("resolutionSimulationHeight", TextBoxScreenHeight.Text);
            Setting.SetAttribute("marginBasic", TextBoxMarginBasic.Text);
            Setting.SetAttribute("marginOverflow", TextBoxMarginOverflow.Text);
            Setting.SetAttribute("textPosition", GetCheckedTextPosition().ToString());
            Setting.SetAttribute("textAlign", (ComboBoxTextAlign.SelectedIndex + 1).ToString());
            Setting.SetAttribute("fontSize", TextBoxFontSize.Text.ToString());
            Setting.SetAttribute("lineHeight", TextBoxLineHeight.Text.ToString());
            Setting.Save();

            if (pw != null) {
                pw.ApplySettings();
            }

            return true;
        }
        private void ButtonApply_Click(object sender, RoutedEventArgs e) {
            Apply();
        }
        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            if (!Apply()) {
                return;
            }
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) {
            mw.IsEnabled = true;
            if (pw != null) {
                pw.IsEnabled = true;
            }
        }

    }
}
