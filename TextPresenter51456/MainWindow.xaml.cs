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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextPresenter51456 {
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window {
        const int PAGE_CANVAS_WIDTH = 224;
        const int PAGE_CANVAS_HEIGHT = 126;

        string filePath;
        string fileName;

        List<string> textList;
        List<int> colorList;
        PageNumberLog pageNum = new PageNumberLog();
        PageNumberManager pvwManager = new PageNumberManager(1, 1, false);
        PageNumberManager pgmManager = new PageNumberManager(0, 0, true);

        SolidColorBrush DEFAULT_BORDER_COLOR = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        SolidColorBrush PVW_BORDER_COLOR = new SolidColorBrush(Color.FromRgb(0, 0xa0, 0));
        SolidColorBrush PGM_BORDER_COLOR = new SolidColorBrush(Color.FromRgb(0xc0, 0, 0));

        PresenterWindow pw;
        HelpWindow hw;

        public MainWindow() {
            InitializeComponent();

            WindowMainWindow.Title = "TextPresenter51456 (Beta) - " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void WindowMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (pw != null) {   // 송출 창 열려 있는 경우
                switch (MessageBox.Show("송출 창이 열려 있습니다. 정말로 종료하시겠습니까?", "TextPresenter51456", MessageBoxButton.YesNo)) {
                case MessageBoxResult.Yes:
                    pw.Close();
                    break;
                default:
                    e.Cancel = true;
                    break;
                }
            }
        }

        private void UpdateBorders() {
            // 항상 pvw -> pgm 순으로 업데이트하므로 UpdatePvw에서 테두리 처리
            foreach (Button item in WrapPanelPageList.Children) {
                item.BorderBrush = DEFAULT_BORDER_COLOR;
            }
            // 페이지리스트는 인덱스가 0부터라 -1
            Button pvwBtn = WrapPanelPageList.Children[pvwManager.PageNumber - 1] as Button;
            pvwBtn.BorderBrush = PVW_BORDER_COLOR;
            if (pgmManager.PageNumber > 0) {
                Button pgmBtn = WrapPanelPageList.Children[pgmManager.PageNumber - 1] as Button;
                pgmBtn.BorderBrush = PGM_BORDER_COLOR;
            }
        }
        private void ClearPgm() {
            pgmManager.PageNumber = 0;
            UpdateBorders();
            PgmContent.Content = "";
            PgmPage.Content = "-";
            if (pw != null) {   // 송출 창 열려 있을 때
                pw.LabelPresenterText.Content = "";
            }
        }
        private SolidColorBrush GetColorFromInt(int c) {
            byte r = (byte)(c / 0x10000 % 0x100);
            byte g = (byte)(c / 0x100 % 0x100);
            byte b = (byte)(c % 0x100);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        private void UpdatePgm() {
            if (textList == null || pgmManager.PageNumber == 0) {
                return;
            }
            SolidColorBrush fg = GetColorFromInt(colorList[pgmManager.PageNumber]);
            PgmContent.Content = textList[pgmManager.PageNumber];
            PgmContent.Foreground = fg;
            PgmPage.Content = pgmManager.PageNumber;
            if (pw != null) {   // 송출 창 열려 있을 때
                pw.LabelPresenterText.Content = textList[pgmManager.PageNumber];
                pw.LabelPresenterText.Foreground = fg;
            }
        }
        private void UpdatePvw() {
            if (textList == null) {
                return;
            }
            UpdateBorders();
            PvwContent.Content = textList[pvwManager.PageNumber];
            PvwContent.Foreground = GetColorFromInt(colorList[pvwManager.PageNumber]);
            PvwPage.Content = pvwManager.PageNumber;
        }

        private void OpenTxtFile() {
            try {   // 선택한 파일 읽기
                System.IO.StreamReader sr = new System.IO.StreamReader(filePath + fileName, Encoding.Default);
                string fullText = sr.ReadToEnd();
                sr.Close();
                Regex newLineUnifier = new Regex(@"(\r\n|\r)");
                Regex trimmer = new Regex(@"(^\n+|\n+$)");
                Regex pageBreaker = new Regex(@"\n{2,}");
                fullText = newLineUnifier.Replace(fullText, "\n"); // 줄바꿈 문자 통일
                fullText = trimmer.Replace(fullText, ""); // 앞뒤 공백 자르기
                textList = new List<string>(pageBreaker.Split(fullText)); // 페이지 나누기
                textList.Insert(0, ""); // 편의상 인덱스 0 비워놓기
                textList.Add(""); // 마지막에 빈 페이지 추가
                pgmManager.LastPageNumber = pvwManager.LastPageNumber = textList.Count - 1;
                WindowMainWindow.Title = "TextPresenter51456 - " + fileName + " (" + filePath + ")";
            } catch (Exception e) {
                string errMsg = "파일 읽기 실패";
                errMsg += "\nMessage: " + e.Message;
                errMsg += "\nSource" + e.Source;
                MessageBox.Show(errMsg, "TextPresenter51456");
                return;
            }
            try {   // 읽어들인 파일 표시하기 + 루프 돌면서 서식 적용
                int len = textList.Count;
                Style pageListItemStyle = FindResource("PageListItem") as Style;
                Regex sharp = new Regex(@"^#+\s*");
                Regex sharpEscaped = new Regex(@"^\\#(#*\s*)");
                SolidColorBrush yellowColorBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));

                colorList = new List<int>(len);
                colorList.Add(0xffffff);

                WrapPanelPageList.Children.Clear();

                for (int i = 1; i < len; i++) {
                    Button pageListItem = new Button();
                    pageListItem.Style = pageListItemStyle;
                    if (sharp.IsMatch(textList[i])) { // # 있으면 자르기
                        textList[i] = sharp.Replace(textList[i], "");
                        colorList.Add(0xffff00);
                        pageListItem.Foreground = yellowColorBrush;
                    } else {
                        colorList.Add(0xffffff);
                    }
                    if (sharpEscaped.IsMatch(textList[i])) { // 이스케이프된 # (\#)
                        textList[i] = sharpEscaped.Replace(textList[i], "#$1");
                    }
                    pageListItem.Content = textList[i];
                    pageListItem.Tag = i;

                    WrapPanelPageList.Children.Add(pageListItem);
                }
                LabelPageIndicator.Content = "/" + (len - 1).ToString();
                UpdatePvw();
            } catch (Exception e) {
                string errMsg = "파일 표시 실패";
                errMsg += "\nMessage: " + e.Message;
                errMsg += "\nSource" + e.Source;
                MessageBox.Show(errMsg, "TextPresenter51456");
            }
        }

        private void SelectTextFile() {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.DefaultExt = ".txt";
            ofd.Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*";

            if (ofd.ShowDialog() == true) {
                fileName = ofd.SafeFileName;
                filePath = ofd.FileName.Remove(ofd.FileName.Length - fileName.Length);
                pvwManager.PageNumber = 1;
                OpenTxtFile();
                MenuItemRefresh.IsEnabled = true;
            }
        }

        /////////////////////////////// 파일 열기
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            SelectTextFile();
        }

        /////////////////////////////// 파일 다시 불러오기
        private void MenuItemRefresh_Click(object sender, RoutedEventArgs e) {
            OpenTxtFile();
        }

        //////////////////////////////// 종료
        private void MenuItemExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnPwUnloaded(object sender, RoutedEventArgs e) {
            pw = null;
        }
        private void OpenPresenterWindow() {
            if (pw == null) {
                // 안 열려 있는 경우
                pw = new PresenterWindow(this);
                pw.Show();
                // 이벤트 핸들러 관리: https://msdn.microsoft.com/ko-kr/library/ms742806(v=vs.110).aspx
                pw.AddHandler(UnloadedEvent, new RoutedEventHandler(OnPwUnloaded));
                UpdatePgm();
            } else {
                MessageBox.Show("이미 송출 창이 열려 있습니다.\n이 메시지를 보고 있다면 개발자에게 알리십시오.", "TextPresenter51456");
            }
        }
        private void ClosePresenterWindow() {
            if (pw != null) {
                // 이미 열려 있는 경우
                pw.Close();
                RoutedEventArgs rea = new RoutedEventArgs(UnloadedEvent);
                rea.Source = pw;
                pw.RaiseEvent(rea);
            } else {
                MessageBox.Show("이미 송출 창이 닫혀 있습니다.\n이 메시지를 보고 있다면 개발자에게 알리십시오.", "TextPresenter51456");
                pw = null;
            }
        }
        private void ButtonPresenterWindow_Checked(object sender, RoutedEventArgs e) {
            OpenPresenterWindow();
        }
        private void ButtonPresenterWindow_Unchecked(object sender, RoutedEventArgs e) {
            ClosePresenterWindow();
        }
        private void OpenAndClosePresenterWindow() {
            if (pw == null) {
                // 안 열려 있는 경우
                // 아래 코드 실행되면서 Checked 이벤트 발생
                ButtonPresenterWindow.IsChecked = true;
                //OpenPresenterWindow();
            } else {
                // 이미 열려 있는 경우
                // 아래 코드 실행되면서 Unchecked 이벤트 발생
                ButtonPresenterWindow.IsChecked = false;
                //ClosePresenterWindow();
            }
        }

        private void UpdatePageIndicator() {
            LabelPageIndicator.Content = pageNum.Log + "/" + pgmManager.LastPageNumber.ToString();
        }
        private void CutAction() {
            int p = pageNum.getPageNumber();
            pageNum.ClearLog();
            if (pvwManager.Enabled) {
                if (p < 0) {
                    // pageNum "": PVW++ -> PGM
                    pgmManager.PageNumber = pvwManager.PageNumber++;
                    UpdatePgm();
                    UpdatePvw();
                } else if (p > 0) {
                    // pageNum에 내용 있음
                    UpdatePageIndicator();
                    pvwManager.PageNumber = p;
                    UpdatePvw();
                } else {
                    // pageNum "0"
                    UpdatePageIndicator();
                    pvwManager.PageNumber -= 1;
                    UpdatePvw();
                }
            }
        }
        private void WindowMainWindow_PreviewKeyDown(object sender, KeyEventArgs e) {   // 키 누를 때 입력 처리
            if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.Escape) {   // Shift+ESC: 송출 창 버튼
                OpenAndClosePresenterWindow();
                return;
            }
            if (MenuItemRefresh.IsEnabled == true && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.R) {
                OpenTxtFile();
                return;
            }
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) {   // 숫자패드
                pageNum.Add(e.Key - Key.NumPad0);
                UpdatePageIndicator();
                return;
            }
            if (e.Key >= Key.D0 && e.Key <= Key.D9) {   // 숫자
                pageNum.Add(e.Key - Key.D0);
                UpdatePageIndicator();
                return;
            }
            switch (e.Key) {
            case Key.Enter:
                CutAction();
                break;
            case Key.Up:
                // 페이지 리스트 스크롤 -130
                ScrollViewerPageList.ScrollToVerticalOffset(ScrollViewerPageList.VerticalOffset - 130);
                break;
            case Key.Down:
                // 페이지 리스트 스크롤 +130
                ScrollViewerPageList.ScrollToVerticalOffset(ScrollViewerPageList.VerticalOffset + 130);
                break;
            case Key.Decimal:
                ClearPgm();
                break;
            case Key.Left:
                pvwManager.PageNumber -= 1;
                UpdatePvw();
                break;
            case Key.Right:
                pvwManager.PageNumber += 1;
                UpdatePvw();
                break;
            }
        }

        private void ScrollViewerPageList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (fileName == null) {   // 아직 파일을 안 불러온 경우
                SelectTextFile();
            }
        }

        private void ScrollViewerPageList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.Source.GetType() == typeof(Button)) {
                pvwManager.PageNumber = int.Parse((e.Source as Button).Tag.ToString());
                UpdatePvw();
                if (e.ClickCount == 2) {
                    pgmManager.PageNumber = pvwManager.PageNumber++;
                    UpdatePgm();
                    UpdatePvw();
                }
            }
        }

        private void WindowMainWindow_MouseDown(object sender, MouseButtonEventArgs e) {
            GridBody.Focus();
            GridBody.Focusable = false;
            GridBody.Focusable = true;
        }

        private void ButtonCut_Click(object sender, RoutedEventArgs e) {
            CutAction();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e) {
            ClearPgm();
        }

        /*
        private void OpenSettings_Click(object sender, RoutedEventArgs e) {
            SettingWindow sw = new SettingWindow(this, pw);
        }
        */

        private void HelpWindow_Unloaded(object sender, RoutedEventArgs e) {
            hw = null;
        }
        private void MenuItemHelp_Click(object sender, RoutedEventArgs e) {
            if (hw == null) {
                if (sender == MenuItemHelpEdit) {
                    hw = new HelpWindow(0);
                    hw.Owner = this;
                    hw.Show();
                } else if (sender == MenuItemHelpPresentation) {
                    hw = new HelpWindow(1);
                    hw.Owner = this;
                    hw.Show();
                } else if (sender == MenuItemInfo) {
                    hw = new HelpWindow(2);
                    hw.Owner = this;
                    hw.Show();
                }
                hw.Unloaded += HelpWindow_Unloaded;
            } else {
                hw.Activate();
                if (sender == MenuItemHelpEdit) {
                    hw.TabControlBody.SelectedIndex = 0;
                } else if (sender == MenuItemHelpPresentation) {
                    hw.TabControlBody.SelectedIndex = 1;
                } else if (sender == MenuItemInfo) {
                    hw.TabControlBody.SelectedIndex = 2;
                }
            }
        }
    }
}
