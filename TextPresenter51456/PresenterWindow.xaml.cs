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
    /// PresenterWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PresenterWindow : Window {
        // Shift+ESC 눌러서 닫기

        Window mw;

        /*
        int presenterScreen, textPosition, textAlign;
        double fontSize, lineHeight;
        */

        public PresenterWindow(Window mw) {
            int presenterScreen;
            this.mw = mw;

            Setting.Load();

            System.Windows.Forms.Screen[] sc = System.Windows.Forms.Screen.AllScreens;
            if (sc.Length < 2) {
                MessageBox.Show("화면 표시 장치가 하나입니다. 이 경우 별도의 설정이 없으면 화면을 모두 덮는 창이 나타나며, Shift+ESC로 닫을 수 있습니다.", "TextPresenter51456");
            }

            if (!int.TryParse(Setting.GetAttribute("presenterScreen"), out presenterScreen)) {
                presenterScreen = System.Windows.Forms.Screen.AllScreens.Length;
            }
            if (presenterScreen > sc.Length) {
                presenterScreen = sc.Length;
            }

            // 설정대로 좌표와 크기를 지정해서 열어야 함
            // 설정 기본값은 마지막 모니터 좌상단, 풀스크린
            System.Drawing.Rectangle r = sc[presenterScreen - 1].Bounds;
            Left = r.Left;
            Top = r.Top;
            Width = r.Width;
            Height = r.Height;

            InitializeComponent();

            ApplySettings();
            
            mw.Activate();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {   // 키보드 입력이 들어오면 MainWindow에서 처리
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt && (e.SystemKey == Key.Space || e.SystemKey == Key.F4)) {
                // Alt+Space, Alt+F4 방지
                e.Handled = true;
                return;
            }
            mw.RaiseEvent(e);
        }

        public void ApplySettings() {
            /* 여기서 다룰 항목
            FontFamily="NanumBarunGothicOTF"
            FontSize="94.5px"
            TextBlock.LineHeight="132.3px"
            TextBlock.TextAlignment="Center"
            Foreground="White"
            HorizontalAlignment="Center"
            VerticalAlignment="Center"
            */

            int presenterScreen, textPosition, textAlign, resolutionSimulationWidth, resolutionSimulationHeight;
            double marginBasic, marginOverflow, fontSize, lineHeight;
            bool resolutionSimulation;

            if (!int.TryParse(Setting.GetAttribute("presenterScreen"), out presenterScreen)) {
                presenterScreen = System.Windows.Forms.Screen.AllScreens.Length;
            }
            if (!double.TryParse(Setting.GetAttribute("marginBasic"), out marginBasic) || marginBasic < 0) {
                marginBasic = 5;
            }
            if (!double.TryParse(Setting.GetAttribute("marginOverflow"), out marginOverflow) || marginOverflow < 0) {
                marginOverflow = 1;
            }
            if (!int.TryParse(Setting.GetAttribute("textPosition"), out textPosition) || textPosition < 1 || textPosition > 9) {
                textPosition = 5;
            }
            if (!int.TryParse(Setting.GetAttribute("textAlign"), out textAlign) || textAlign < 1 || textAlign > 3) {
                textAlign = 2;
            }
            if (!double.TryParse(Setting.GetAttribute("fontSize"), out fontSize) || fontSize <= 0) {
                fontSize = 8.75;
            }
            if (!double.TryParse(Setting.GetAttribute("lineHeight"), out lineHeight) || lineHeight < 0) {
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

            // 해상도 시뮬레이션
            if (resolutionSimulation) {
                GridExCol1.Width = new GridLength(1, GridUnitType.Star);
                GridExCol2.Width = new GridLength(resolutionSimulationWidth, GridUnitType.Pixel);
                GridExCol3.Width = new GridLength(1, GridUnitType.Star);
                GridExRow1.Height = new GridLength(1, GridUnitType.Star);
                GridExRow2.Height = new GridLength(resolutionSimulationHeight, GridUnitType.Pixel);
                GridExRow3.Height = new GridLength(1, GridUnitType.Star);
                fontSize = fontSize * resolutionSimulationHeight / Height;
            } else {
                GridExCol1.Width = new GridLength(0, GridUnitType.Pixel);
                GridExCol2.Width = new GridLength(1, GridUnitType.Star);
                GridExCol3.Width = new GridLength(0, GridUnitType.Pixel);
                GridExRow1.Height = new GridLength(0, GridUnitType.Pixel);
                GridExRow2.Height = new GridLength(1, GridUnitType.Star);
                GridExRow3.Height = new GridLength(0, GridUnitType.Pixel);
            }

            // 단위 보정
            fontSize /= 100;
            lineHeight /= 100;


            // FontFamily
            try {
                LabelPresenterText.FontFamily = new FontFamily("NanumBarunGothic");
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                LabelPresenterText.FontFamily = new FontFamily("Malgun Gothic");
            }

            // FontSize
            LabelPresenterText.FontSize = RelativeToAbsolute.MakeAbsolute(fontSize, Height);

            // TextBlock.LineHeight
            LabelPresenterText.SetValue(TextBlock.LineHeightProperty, RelativeToAbsolute.MakeAbsolute(fontSize * lineHeight, Height));

            // TextBlock.TextAlignment TextBlock.TextAlignment="Center"
            switch (textAlign) {
                case 1: // Left
                    LabelPresenterText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);
                    break;
                case 3: // Right
                    LabelPresenterText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
                    break;
                default: // 2 또는 invalid value -> Center
                    LabelPresenterText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    break;
            }

            // Foreground
            LabelPresenterText.Foreground = Brushes.White; //new SolidColorBrush(Color.FromRgb(255, 255, 255));

            // Alignment
            if ((textPosition - 1) / 3 == 0) {
                // 1, 2, 3 -> Top
                LabelPresenterText.VerticalAlignment = VerticalAlignment.Top;
                GridRow1.Height = new GridLength(marginBasic, GridUnitType.Star);
                GridRow2.Height = new GridLength(100 - marginBasic - marginOverflow, GridUnitType.Star);
                GridRow3.Height = new GridLength(marginOverflow, GridUnitType.Star);
            } else if ((textPosition - 1) / 3 == 2) {
                // 7, 8, 9 -> Bottom
                LabelPresenterText.VerticalAlignment = VerticalAlignment.Bottom;
                GridRow1.Height = new GridLength(marginOverflow, GridUnitType.Star);
                GridRow2.Height = new GridLength(100 - marginBasic - marginOverflow, GridUnitType.Star);
                GridRow3.Height = new GridLength(marginBasic, GridUnitType.Star);
            } else {
                // 4, 5, 6, invalid value -> Center
                LabelPresenterText.VerticalAlignment = VerticalAlignment.Center;
                GridRow1.Height = new GridLength(marginOverflow, GridUnitType.Star);
                GridRow2.Height = new GridLength(100 - 2 * marginOverflow, GridUnitType.Star);
                GridRow3.Height = new GridLength(marginOverflow, GridUnitType.Star);
            }
            if (textPosition % 3 == 1) {
                // 1, 4, 7 -> Left
                LabelPresenterText.HorizontalAlignment = HorizontalAlignment.Left;
                GridCol1.Width = new GridLength(marginBasic, GridUnitType.Star);
                GridCol2.Width = new GridLength(100 - marginBasic - marginOverflow, GridUnitType.Star);
                GridCol3.Width = new GridLength(marginOverflow, GridUnitType.Star);
            } else if (textPosition % 3 == 0) {
                // 3, 6, 9 -> Right
                LabelPresenterText.HorizontalAlignment = HorizontalAlignment.Right;
                GridCol1.Width = new GridLength(marginOverflow, GridUnitType.Star);
                GridCol2.Width = new GridLength(100 - marginBasic - marginOverflow, GridUnitType.Star);
                GridCol3.Width = new GridLength(marginBasic, GridUnitType.Star);
            } else {
                // 2, 5, 8, invalid value -> Center
                LabelPresenterText.HorizontalAlignment = HorizontalAlignment.Center;
                GridCol1.Width = new GridLength(marginOverflow, GridUnitType.Star);
                GridCol2.Width = new GridLength(100 - 2 * marginOverflow, GridUnitType.Star);
                GridCol3.Width = new GridLength(marginOverflow, GridUnitType.Star);
            }
        }

    }
}
