using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        int presenterScreen, textEncoding, textPosition, textAlign, resolutionSimulationWidth, resolutionSimulationHeight, titleColor;
        decimal marginBasic, marginOverflow, fontSize, lineHeight;
        bool resolutionSimulation, fontWeightBold = false, fontStyleItalic = false;
        string fontFamily;

        Thickness borderThickness = new Thickness(1.0);

        readonly Regex notHexText = new Regex("[^0-9A-Fa-f]+");
        readonly Regex notPositiveRealText = new Regex("[^0-9.]+");
        readonly Regex notPositiveIntText = new Regex("[^0-9]+");


        // Load current settings from Setting class
        private void GetSettings() {
            if (!int.TryParse(Setting.GetAttribute("presenterScreen"), out presenterScreen)) {
                presenterScreen = System.Windows.Forms.Screen.AllScreens.Length;
            }
            if (!int.TryParse(Setting.GetAttribute("textEncoding"), out textEncoding)) {
                textEncoding = 0;
            }
            switch (textEncoding) {
                case 0: // default
                case 1200: // UTF-16 LE
                case 1201: // UTF-16 BE
                case 65001: // UTF-8
                    break;
                default:
                    textEncoding = 0;
                    break;
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
            if ((fontFamily = Setting.GetAttribute("fontFamily")) == null) {
                fontFamily = "Malgun Gothic";
            }
            if (Setting.GetAttribute("fontWeight").Equals("bold")) {
                fontWeightBold = true;
            }
            if (Setting.GetAttribute("fontStyle").Equals("italic")) {
                fontStyleItalic = true;
            }
            if (!decimal.TryParse(Setting.GetAttribute("fontSize"), out fontSize) || fontSize <= 0) {
                fontSize = 8.75m;
            }
            try {
                titleColor = MiscConverter.StringToIntColor(Setting.GetAttribute("titleColor"));
            } catch (Exception ex) {
                // invalid color format
                Console.WriteLine(ex.Message);
                titleColor = 0xffff00;
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

        private void UpdateButtonChangeFont() {
            string fontInfo;
            TextBlock tb = new TextBlock();

            fontInfo = fontFamily;
            if (fontWeightBold && fontStyleItalic) {
                fontInfo += " (굵게, 기울임)";
            } else if (fontWeightBold) {
                fontInfo += " (굵게)";
            } else if (fontStyleItalic) {
                fontInfo += " (기울임)";
            } else {
                fontInfo += " (보통)";
            }
            tb.Text = fontInfo;
            ButtonChangeFont.Content = tb;
            ButtonChangeFont.FontFamily = new FontFamily(fontFamily);
            ButtonChangeFont.FontWeight = fontWeightBold ? FontWeights.Bold : FontWeights.Regular;
            ButtonChangeFont.FontStyle = fontStyleItalic ? FontStyles.Italic : FontStyles.Normal;
        }

        // Get and apply settings from Setting class to SettingWindow controls
        private void SettingToControl() {
            System.Windows.Forms.Screen[] sc = System.Windows.Forms.Screen.AllScreens;
            int numOfScreen = sc.Length;
            int fullWidth = 0, fullHeight = 0;
            double threshold = 6.25; // 500 / 80 = 6.25
            double multiplyFactor = 1.0;

            GetSettings();

            // for drawing screen map
            // get full size of all of the monitors
            for (int i = 0; i < numOfScreen; i++) {
                System.Drawing.Rectangle r = sc[i].Bounds;
                int ithWidth = r.X + r.Width;
                int ithHeight = r.Y + r.Height;
                if (fullWidth < ithWidth) {
                    fullWidth = ithWidth;
                }
                if (fullHeight < ithHeight) {
                    fullHeight = ithHeight;
                }
            }
            // calculate multiply factor
            if ((double)fullWidth / fullHeight <= threshold) {
                multiplyFactor = 80.0 / fullHeight;
            } else {
                multiplyFactor = 500.0 / fullWidth;
            }

            // construct screen combo box and draw screen map
            ComboBoxPresenterScreen.Items.Clear();
            CanvasScreens.Children.Clear();
            for (int i = 0; i < numOfScreen; i++) {
                System.Drawing.Rectangle r = sc[i].Bounds;

                // combo box item
                ComboBoxItem cbi = new ComboBoxItem {
                    // content of item example: (2) 1024x768 (1920,0)
                    Content = "(" + (i + 1).ToString() + ") " + r.Width.ToString() + "×" + r.Height.ToString() + " (" + r.X.ToString() + "," + r.Y.ToString() + ")"
                };
                ComboBoxPresenterScreen.Items.Add(cbi);

                // screen map item
                TextBlock tb = new TextBlock {
                    Text = (i + 1).ToString(),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 16.0,
                    FontWeight = FontWeight.FromOpenTypeWeight(700)
                };
                Border b = new Border {
                    Width = r.Width * multiplyFactor,
                    Height = r.Height * multiplyFactor,
                    BorderBrush = Brushes.Black,
                    BorderThickness = borderThickness,
                    Child = tb
                };
                Canvas.SetLeft(b, 5 + r.X * multiplyFactor);
                Canvas.SetTop(b, 5 + r.Y * multiplyFactor);
                CanvasScreens.Children.Add(b);
            }
            if (presenterScreen <= numOfScreen) {
                ComboBoxPresenterScreen.SelectedIndex = presenterScreen - 1;
            } else {
                ComboBoxPresenterScreen.SelectedIndex = numOfScreen - 1;
            }

            // textEncoding = ComboBoxTextEncoding.SelectedIndex;
            switch (textEncoding) {
                case 1200: // UTF-16 LE
                    ComboBoxTextEncoding.SelectedIndex = 2;
                    break;
                case 1201: // UTF-16 BE
                    ComboBoxTextEncoding.SelectedIndex = 3;
                    break;
                case 65001: // UTF-8
                    ComboBoxTextEncoding.SelectedIndex = 1;
                    break;
                default:
                    ComboBoxTextEncoding.SelectedIndex = 0;
                    break;
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
            UpdateButtonChangeFont();
            TextBoxFontSize.Text = fontSize.ToString();
            TextBoxTitleColor.Text = titleColor.ToString("X6");
            RectangleTitleColor.Fill = MiscConverter.IntToSolidColorBrush(titleColor);
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

        private void TextBoxTitleColor_TextChanged(object sender, TextChangedEventArgs e) {
            if (TextBoxTitleColor.Text.Count() == 6) {
                RectangleTitleColor.Fill = MiscConverter.IntToSolidColorBrush(MiscConverter.StringToIntColor(TextBoxTitleColor.Text));
            }
        }

        private void TextBoxDouble_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = notPositiveRealText.IsMatch(e.Text);
        }
        private void TextBoxInt_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = notPositiveIntText.IsMatch(e.Text);
        }
        private void TextBoxHex_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = notHexText.IsMatch(e.Text);
        }

        private void ButtonChangeFont_Click(object sender, RoutedEventArgs e) {
            System.Drawing.FontStyle fs = (fontWeightBold ? System.Drawing.FontStyle.Bold : 0) | (fontStyleItalic ? System.Drawing.FontStyle.Italic : 0);
            System.Drawing.Font font = new System.Drawing.Font(fontFamily, 16, fs);
            System.Windows.Forms.FontDialog fontDialog = new System.Windows.Forms.FontDialog {
                AllowVerticalFonts = false,
                Font = font,
                ShowApply = false,
                ShowEffects = false
            };
            if (fontDialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel) {
                fontFamily = fontDialog.Font.Name;
                fontWeightBold = fontDialog.Font.Bold;
                fontStyleItalic = fontDialog.Font.Italic;

                UpdateButtonChangeFont();
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
            int tempInt;
            decimal tempDec;
            if (!int.TryParse(TextBoxScreenWidth.Text, out tempInt) || tempInt <= 0) {
                ShowWrongFormatMessage("해상도 시뮬레이션 가로 길이", "양의 정수");
                return false;
            }
            if (!int.TryParse(TextBoxScreenHeight.Text, out tempInt) || tempInt <= 0) {
                ShowWrongFormatMessage("해상도 시뮬레이션 세로 길이", "양의 정수");
                return false;
            }
            if (!decimal.TryParse(TextBoxFontSize.Text, out tempDec) || tempDec <= 0) {
                ShowWrongFormatMessage("텍스트 크기", "양수");
                return false;
            }
            if (notHexText.IsMatch(TextBoxTitleColor.Text)) {
                ShowWrongFormatMessage("제목 색상", "6자리 16진수 색상 코드(rrggbb)");
                return false;
            }
            if (!decimal.TryParse(TextBoxLineHeight.Text, out tempDec) || tempDec <= 0) {
                ShowWrongFormatMessage("줄 간격", "양수");
                return false;
            }
            if (!decimal.TryParse(TextBoxMarginBasic.Text, out tempDec) || tempDec < 0) {
                ShowWrongFormatMessage("기본 여백", "0 또는 양수");
                return false;
            }
            if (!decimal.TryParse(TextBoxMarginOverflow.Text, out tempDec) || tempDec < 0) {
                ShowWrongFormatMessage("넘치는 부분 여백", "0 또는 양수");
                return false;
            }

            switch (ComboBoxTextEncoding.SelectedIndex) {
                case 1: // UTF-8
                    textEncoding = 65001;
                    break;
                case 2: // UTF-16 LE
                    textEncoding = 1200;
                    break;
                case 3: // UTF-16 BE
                    textEncoding = 1201;
                    break;
                default:
                    textEncoding = 0;
                    break;
            }

            Setting.SetAttribute("fontFamily", fontFamily);
            Setting.SetAttribute("fontSize", TextBoxFontSize.Text.Trim());
            Setting.SetAttribute("fontStyle", fontStyleItalic ? "italic" : "normal");
            Setting.SetAttribute("fontWeight", fontWeightBold ? "bold" : "regular");
            Setting.SetAttribute("lineHeight", TextBoxLineHeight.Text.Trim());
            Setting.SetAttribute("marginBasic", TextBoxMarginBasic.Text.Trim());
            Setting.SetAttribute("marginOverflow", TextBoxMarginOverflow.Text.Trim());
            Setting.SetAttribute("presenterScreen", (ComboBoxPresenterScreen.SelectedIndex + 1).ToString());
            Setting.SetAttribute("resolutionSimulation", CheckBoxResolutionSimulation.IsChecked.ToString());
            Setting.SetAttribute("resolutionSimulationHeight", TextBoxScreenHeight.Text.Trim());
            Setting.SetAttribute("resolutionSimulationWidth", TextBoxScreenWidth.Text.Trim());
            Setting.SetAttribute("textAlign", (ComboBoxTextAlign.SelectedIndex + 1).ToString());
            Setting.SetAttribute("textEncoding", textEncoding.ToString());
            Setting.SetAttribute("textPosition", GetCheckedTextPosition().ToString());
            Setting.SetAttribute("titleColor", TextBoxTitleColor.Text.Trim());
            Setting.Save();

            if (pw != null) {
                pw.ApplySettings();
            }
            mw.GetTitleColor();

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
            mw.Focus();
        }

    }
}
