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
        bool usePvw, useMouse, useKeyboard, updateOnChange;
        int presenterScreen, textPosition, textAlign;
        double fontSize, lineHeight;
        */

        public PresenterWindow(Window w1) {
            mw = w1;

            System.Windows.Forms.Screen[] sc = System.Windows.Forms.Screen.AllScreens;
            if (sc.Length < 2) {
                MessageBox.Show("화면 표시 장치가 하나입니다. 이 경우 별도의 설정이 없으면 화면을 모두 덮는 창이 나타나며, Shift+ESC로 닫을 수 있습니다.", "TextPresenter51456");
            }

            // 설정대로 좌표와 크기를 지정해서 열어야 함
            // 설정 기본값은 2번째 모니터 좌상단, 풀스크린
            System.Drawing.Rectangle r = sc[sc.Length - 1].Bounds;
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

        private void ApplySettings() {
            /* 여기서 다룰 항목
            FontFamily="NanumBarunGothicOTF"
            FontSize="94.5px"
            TextBlock.LineHeight="132.3px"
            TextBlock.TextAlignment="Center"
            Foreground="White"
            HorizontalAlignment="Center"
            VerticalAlignment="Center"
            */
            
            /*
            if (!int.TryParse(Setting.Get("presenterScreen"), out presenterScreen)) {
                presenterScreen = 2;
            }
            if (!int.TryParse(Setting.Get("textPosition"), out textPosition)) {
                textPosition = 5;
            }
            if (!int.TryParse(Setting.Get("textAlign"), out textAlign)) {
                textAlign = 2;
            }
            if (!double.TryParse(Setting.Get("fontSize"), out fontSize)) {
                fontSize = 2;
            }
            if (!double.TryParse(Setting.Get("lineHeight"), out lineHeight)) {
                lineHeight = 2;
            }
            */

            // FontFamily
            try {
                LabelPresenterText.FontFamily = new FontFamily("NanumBarunGothic");
            } catch (Exception e) {
                LabelPresenterText.FontFamily = new FontFamily("Malgun Gothic");
            }

            // FontSize
            LabelPresenterText.FontSize = RelativeToAbsolute.MakeAbsolute(0.0875, Height);

            // TextBlock.LineHeight
            LabelPresenterText.SetValue(TextBlock.LineHeightProperty, RelativeToAbsolute.MakeAbsolute(0.0875 * 1.4, Height));

            // TextBlock.TextAlignment TextBlock.TextAlignment="Center"
            LabelPresenterText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);

            // Foreground
            LabelPresenterText.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            // Alignment
            LabelPresenterText.HorizontalAlignment = HorizontalAlignment.Center;
            LabelPresenterText.VerticalAlignment = VerticalAlignment.Center;
        }
        private void Window_LoadedEvent(object sender, RoutedEventArgs e) {
            ApplySettings();
        }
    }
}
