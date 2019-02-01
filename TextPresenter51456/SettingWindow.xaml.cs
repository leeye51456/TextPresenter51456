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

        int presenterScreen, textEncoding, textPosition, textAlign, screenRatioSimulationWidth, screenRatioSimulationHeight, titleColor;
        decimal marginBasic, marginOverflow, fontSize, lineHeight;
        bool screenRatioSimulation, fontWeightBold = false, fontStyleItalic = false;
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
                string tempTitleColor = Setting.GetAttribute("titleColor");
                if (tempTitleColor == null) {
                    titleColor = 0xffff00;
                } else {
                    titleColor = MiscConverter.StringToIntColor(tempTitleColor);
                }
            } catch (Exception ex) {
                // invalid color format
                Console.WriteLine(ex.Message);
                titleColor = 0xffff00;
            }
            if (!decimal.TryParse(Setting.GetAttribute("lineHeight"), out lineHeight) || lineHeight < 0) {
                lineHeight = 140;
            }
            if (!bool.TryParse(Setting.GetAttribute("screenRatioSimulation"), out screenRatioSimulation)) {
                screenRatioSimulation = false;
            }
            if (!int.TryParse(Setting.GetAttribute("screenRatioSimulationWidth"), out screenRatioSimulationWidth) || screenRatioSimulationWidth <= 0) {
                screenRatioSimulationWidth = 4;
            }
            if (!int.TryParse(Setting.GetAttribute("screenRatioSimulationHeight"), out screenRatioSimulationHeight) || screenRatioSimulationHeight <= 0) {
                screenRatioSimulationHeight = 3;
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

        private void ScreenBorder_Click(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Primitives.ToggleButton tb = sender as System.Windows.Controls.Primitives.ToggleButton;
            int t = (int)tb.Tag;
            int len = ComboBoxPresenterScreen.Items.Count;
            for (int i = 0; i < len; i++) {
                (CanvasScreens.Children[i] as System.Windows.Controls.Primitives.ToggleButton).IsChecked = false;
            }
            tb.IsChecked = true;
            ComboBoxPresenterScreen.SelectedIndex = t;
        }
        private void ComboBoxPresenterScreen_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int len = ComboBoxPresenterScreen.Items.Count;
            for (int i = 0; i < len; i++) {
                (CanvasScreens.Children[i] as System.Windows.Controls.Primitives.ToggleButton).IsChecked = false;
            }
            (CanvasScreens.Children[ComboBoxPresenterScreen.SelectedIndex] as System.Windows.Controls.Primitives.ToggleButton).IsChecked = true;
        }

        // Get and apply settings from Setting class to SettingWindow controls
        private void SettingToControl() {
            System.Windows.Forms.Screen[] sc = System.Windows.Forms.Screen.AllScreens;
            int numOfScreen = sc.Length;
            double fullWidth = 0, fullHeight = 0, offsetX = 0, offsetY = 0;
            double dpiX = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11;
            double dpiY = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M22;
            double threshold = 6.25; // 500 / 80 = 6.25
            double multiplyFactor = 1.0;

            List<ScreenRect> actualScreenRectList = new List<ScreenRect>(numOfScreen);

            GetSettings();

            // for drawing screen map
            // get full size of all of the monitors
            // and get x, y offsets for negative positions
            for (int i = 0; i < numOfScreen; i++) {
                System.Drawing.Rectangle r = sc[i].Bounds;

                if (sc[i].Primary) {
                    if (offsetX > r.X) {
                        offsetX = r.X;
                    }
                    if (offsetY > r.Y) {
                        offsetY = r.Y;
                    }
                    if (fullWidth < r.Width) {
                        fullWidth = r.X + r.Width;
                    }
                    if (fullHeight < r.Height) {
                        fullHeight = r.Y + r.Height;
                    }
                    actualScreenRectList.Add(new ScreenRect(r.X, r.Y, r.Width, r.Height));
                } else {
                    double ithX = r.X / dpiX;
                    double ithY = r.Y / dpiY;
                    double ithWidth = r.Width / dpiX;
                    double ithHeight = r.Height / dpiY;
                    double ithFullWidth = ithX + ithWidth;
                    double ithFullHeight = ithY + ithHeight;
                    if (offsetX > ithX) {
                        offsetX = ithX;
                    }
                    if (offsetY > ithY) {
                        offsetY = ithY;
                    }
                    if (fullWidth < ithFullWidth) {
                        fullWidth = ithFullWidth;
                    }
                    if (fullHeight < ithFullHeight) {
                        fullHeight = ithFullHeight;
                    }
                    actualScreenRectList.Add(new ScreenRect(ithX, ithY, ithWidth, ithHeight));
                }
            }
            offsetX *= -1;
            offsetY *= -1;
            fullWidth += offsetX;
            fullHeight += offsetY;
            // calculate multiply factor
            if (fullWidth / fullHeight <= threshold) {
                multiplyFactor = 80.0 / fullHeight;
            } else {
                multiplyFactor = 500.0 / fullWidth;
            }

            // construct screen combo box and draw screen map
            ComboBoxPresenterScreen.Items.Clear();
            CanvasScreens.Children.Clear();
            for (int i = 0; i < numOfScreen; i++) {
                System.Drawing.Rectangle r = sc[i].Bounds;
                double ithWidth = actualScreenRectList[i].width;
                double ithHeight = actualScreenRectList[i].height;
                double ithX = actualScreenRectList[i].x;
                double ithY = actualScreenRectList[i].y;

                // combo box item
                ComboBoxItem cbi = new ComboBoxItem {
                    Content = "(" + (i + 1).ToString() + ") " + sc[i].DeviceName + ": " + ithWidth.ToString() + "×" + ithHeight.ToString() + " (" + ithX.ToString() + "," + ithY.ToString() + ")"
                };
                ComboBoxPresenterScreen.Items.Add(cbi);

                // screen map item as button
                Style screenBorderStyle = FindResource("ScreenBorder") as Style;
                System.Windows.Controls.Primitives.ToggleButton btn = new System.Windows.Controls.Primitives.ToggleButton {
                    Style = screenBorderStyle,
                    Content = (i + 1).ToString(),
                    Width = ithWidth * multiplyFactor,
                    Height = ithHeight * multiplyFactor,
                    Tag = i,
                };
                btn.Click += ScreenBorder_Click;
                Canvas.SetLeft(btn, 5 + (ithX + offsetX) * multiplyFactor);
                Canvas.SetTop(btn, 5 + (ithY + offsetY) * multiplyFactor);
                CanvasScreens.Children.Add(btn);
            }
            if (presenterScreen <= numOfScreen) {
                ComboBoxPresenterScreen.SelectedIndex = presenterScreen - 1;
            } else {
                ComboBoxPresenterScreen.SelectedIndex = numOfScreen - 1;
            }
            (CanvasScreens.Children[ComboBoxPresenterScreen.SelectedIndex] as System.Windows.Controls.Primitives.ToggleButton).IsChecked = true;

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

            CheckBoxScreenRatioSimulation.IsChecked = screenRatioSimulation;
            TextBoxScreenWidth.Text = screenRatioSimulationWidth.ToString();
            TextBoxScreenHeight.Text = screenRatioSimulationHeight.ToString();
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

        private void ButtonTitleColor_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog {
                FullOpen = true
            };
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                TextBoxTitleColor.Text = (cd.Color.R * 0x10000 + cd.Color.G * 0x100 + cd.Color.B).ToString("X6");
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

        private void TextBoxes_GotFocus(object sender, RoutedEventArgs e) {
            (sender as TextBox).SelectAll();
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
            Setting.SetAttribute("screenRatioSimulation", CheckBoxScreenRatioSimulation.IsChecked.ToString());
            Setting.SetAttribute("screenRatioSimulationHeight", TextBoxScreenHeight.Text.Trim());
            Setting.SetAttribute("screenRatioSimulationWidth", TextBoxScreenWidth.Text.Trim());
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

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                ButtonCancel_Click(ButtonCancel, null);
            }
        }

    }
}
