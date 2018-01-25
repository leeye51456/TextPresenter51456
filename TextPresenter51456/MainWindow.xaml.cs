using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        Regex newLineUnifier = new Regex(@"(\r\n|\r)");
        Regex pageBreaker = new Regex(@"\n{2,}");
        Regex sharp = new Regex(@"^#+\s*");
        Regex sharpEscaped = new Regex(@"^\\#(#*\s*)");
        Regex trimmer = new Regex(@"(^\n+|\n+$)");
        Regex fileNameTrimmer = new Regex(@"^(.*\\)(.*?)$");

        SolidColorBrush DEFAULT_BORDER_COLOR = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        SolidColorBrush PVW_BORDER_COLOR = new SolidColorBrush(Color.FromRgb(0, 0xa0, 0));
        SolidColorBrush PGM_BORDER_COLOR = new SolidColorBrush(Color.FromRgb(0xc0, 0, 0));

        PresenterWindow pw;
        SettingWindow sw;
        HelpWindow hw;

        Thread threadRemote;
        string processRemoteReturn;


        public MainWindow() {
            SynSocketListener.mw = this;
            Setting.Load();

            InitializeComponent();

            WindowMainWindow.Title = "TextPresenter51456 (Beta) - " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void KillFocus() {
            GridBody.Focus();
        }

        private void GridBody_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            KillFocus();
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
            try {
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
            } catch (Exception ex) {
                // 페이지리스트에 버튼 컨트롤이 아닌 게 있을 때: 초기 상태에서
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

        private void OpenTxtFile(bool resetPvw) {
            if (resetPvw) {
                pvwManager.PageNumber = 1;
            }
            try {   // 선택한 파일 읽기
                System.IO.StreamReader sr = new System.IO.StreamReader(filePath + fileName, Encoding.Default);
                string fullText = sr.ReadToEnd();
                sr.Close();
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
                SolidColorBrush yellowColorBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));

                colorList = new List<int>(len);
                colorList.Add(0xffffff);

                WrapPanelPageList.Children.Clear();

                // 페이지 리스트 만들기
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

        private bool SetFileNameAndPath(string src) {
            // 1. txt 파일 -> 통과
            // 2. non-txt 파일 & Yes -> 통과
            // 3. non-txt 파일 & No -> 취소
            if (!src.ToLower().EndsWith(".txt")
                && MessageBox.Show(
                    WindowMainWindow,
                    "txt 파일이 아닌 경우 제대로 열리지 않거나 의도하지 않은 대로 동작할 수 있습니다.\n그래도 여시겠습니까?",
                    "경고",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No) == MessageBoxResult.No) {
                return false;
            }
            fileName = fileNameTrimmer.Replace(src, "$2");
            filePath = fileNameTrimmer.Replace(src, "$1");
            return true;
        }

        private void SelectTextFile() {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.DefaultExt = ".txt";
            ofd.Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*";

            if (ofd.ShowDialog() == true) {
                if (!SetFileNameAndPath(ofd.FileName)) {
                    // 취소한 경우
                    return;
                }
                OpenTxtFile(true);
                MenuItemRefresh.IsEnabled = true;
            }
        }

        private void UpdateFree() {
            string text = newLineUnifier.Replace(TextBoxFreeContent.Text, "\n"); // 줄바꿈 문자 통일
            text = trimmer.Replace(text, ""); // 앞뒤 공백 자르기

            SolidColorBrush fg = GetColorFromInt(0xffffff); // 일단 무조건 흰색
            //SolidColorBrush fg = GetColorFromInt(colorList[pgmManager.PageNumber]);
            PgmContent.Content = text;
            PgmContent.Foreground = fg;
            if (pw != null) {   // 송출 창 열려 있을 때
                pw.LabelPresenterText.Content = text;
                pw.LabelPresenterText.Foreground = fg;
            }
        }
        private void FreeCut() {
            pgmManager.PageNumber = 0;
            PgmPage.Content = "자유송출";
            UpdateBorders();
            UpdateFree();
        }

        /////////////////////////////// 파일 드래그
        private void WindowMainWindow_PreviewDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (!SetFileNameAndPath(files[0])) {
                    return;
                }
                OpenTxtFile(true);
            }
        }

        private void WindowMainWindow_PreviewDragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Copy;
            } else {
                e.Effects = DragDropEffects.None;
            }
        }

        /////////////////////////////// 파일 열기
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            SelectTextFile();
        }

        /////////////////////////////// 파일 다시 불러오기
        private void MenuItemRefresh_Click(object sender, RoutedEventArgs e) {
            OpenTxtFile(false);
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
        private bool OpenAndClosePresenterWindow() {
            if (pw == null) {
                // 안 열려 있는 경우
                // 아래 코드 실행되면서 Checked 이벤트 발생
                ButtonPresenterWindow.IsChecked = true;
                //OpenPresenterWindow();
                return true;
            } else {
                // 이미 열려 있는 경우
                // 아래 코드 실행되면서 Unchecked 이벤트 발생
                ButtonPresenterWindow.IsChecked = false;
                //ClosePresenterWindow();
                return false;
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

        private void WindowMainWindow_PreviewKeyDown(object sender, KeyEventArgs e) { // 키 누를 때 입력 처리
            if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.Escape) { // Shift+ESC: 송출 창 버튼
                OpenAndClosePresenterWindow();
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter) { // Ctrl+Enter: 자유 송출
                FreeCut();
                return;
            }
            if (MenuItemRefresh.IsEnabled == true && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.R) { // Ctrl+R: 리로드
                OpenTxtFile(false);
                return;
            }

            if (TextBoxFreeContent.IsFocused) { // 자유 송출 텍스트 상자 포커스
                switch (e.Key) {
                case Key.Escape:
                    KillFocus();
                    break;
                case Key.OemPeriod: // 일반 .키
                case Key.Decimal: // 키패드 .키
                    if (Keyboard.Modifiers == ModifierKeys.Control) {
                        ClearPgm();
                    }
                    break;
                }
            } else {
                if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) { // 숫자패드
                    pageNum.Add(e.Key - Key.NumPad0);
                    UpdatePageIndicator();
                    return;
                }
                if (e.Key >= Key.D0 && e.Key <= Key.D9) { // 숫자
                    pageNum.Add(e.Key - Key.D0);
                    UpdatePageIndicator();
                    return;
                }
                switch (e.Key) {
                case Key.Enter:
                    CutAction();
                    KillFocus();
                    e.Handled = true;
                    break;
                case Key.Up:
                    ScrollViewerPageList.ScrollToVerticalOffset(ScrollViewerPageList.VerticalOffset - 130);
                    KillFocus();
                    e.Handled = true;
                    break;
                case Key.Down:
                    ScrollViewerPageList.ScrollToVerticalOffset(ScrollViewerPageList.VerticalOffset + 130);
                    KillFocus();
                    e.Handled = true;
                    break;
                case Key.OemPeriod: // 일반 .키
                case Key.Decimal: // 키패드 .키
                    ClearPgm();
                    KillFocus();
                    e.Handled = true;
                    break;
                case Key.Left:
                    pvwManager.PageNumber -= 1;
                    UpdatePvw();
                    KillFocus();
                    e.Handled = true;
                    break;
                case Key.Right:
                    pvwManager.PageNumber += 1;
                    UpdatePvw();
                    KillFocus();
                    e.Handled = true;
                    break;
                }
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
            KillFocus();
        }

        private void ButtonCut_Click(object sender, RoutedEventArgs e) {
            CutAction();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e) {
            ClearPgm();
        }

        private void ButtonFreeCut_Click(object sender, RoutedEventArgs e) {
            FreeCut();
        }
        
        private void MenuItemSettings_Click(object sender, RoutedEventArgs e) {
            sw = new SettingWindow(this, pw);
            sw.Owner = this;
            sw.Show();
            if (pw != null) {
                pw.IsEnabled = false;
            }
            this.IsEnabled = false;
        }
        private void MenuItemReloadSettings_Click(object sender, RoutedEventArgs e) {
            Setting.Load();
            pw.ApplySettings();
        }

        private void MenuItemHelp_Click(object sender, RoutedEventArgs e) {
            if (sender == MenuItemHelpEdit) {
                hw = new HelpWindow(0, this, pw);
                hw.Owner = this;
                hw.Show();
            } else if (sender == MenuItemHelpPresentation) {
                hw = new HelpWindow(1, this, pw);
                hw.Owner = this;
                hw.Show();
            } else if (sender == MenuItemInfo) {
                hw = new HelpWindow(2, this, pw);
                hw.Owner = this;
                hw.Show();
            }
            if (pw != null) {
                pw.IsEnabled = false;
            }
            this.IsEnabled = false;
        }


        private string PackPageListToString() {
            string result = "";
            /*
             *  int colorList[i]
             *  string textList[i]
             *  
             *  int colorList[i + 1]
             *  string textList[i + 1]
             *  
             *  ...
             */
            if (textList.Count < 1) {
                return result;
            }
            result += colorList[1].ToString() + "\n" + textList[1];
            for (int i = 2; i < textList.Count; i++) {
                result += "\n\n" + colorList[i].ToString() + "\n" + textList[i];
            }
            return result;
        }
        private void ProcessRemote(string cmd, string param) {
            processRemoteReturn = "";

            if (cmd.Equals("pgm")) {
                // PGM 변경
                // SEND   pgm:page<EndOfCommand>
                // RETURN -
                if (int.TryParse(param, out int pageNum)) {
                    // 정상적인 경우
                    pgmManager.PageNumber = pageNum;
                    pvwManager.PageNumber = pageNum + 1;
                    UpdatePgm();
                    UpdatePvw();
                }

                processRemoteReturn = "pgm:" + pgmManager.PageNumber + "<EndOfCommand>";

            } else if (cmd.Equals("pvw")) {
                // PVW 변경
                // SEND   pvw:page<EndOfCommand>
                // RETURN -
                if (int.TryParse(param, out int pageNum)) {
                    // 정상적인 경우
                    pvwManager.PageNumber = pageNum;
                    UpdatePvw();
                }

                processRemoteReturn = "pvw:" + pvwManager.PageNumber + "<EndOfCommand>";

            } else if (cmd.Equals("cut")) {
                // 컷
                // SEND   cut:<EndOfCommand>
                // RETURN -
                CutAction();

                processRemoteReturn = "cut:" + pgmManager.PageNumber + "<EndOfCommand>";

            } else if (cmd.Equals("clear")) {
                // 화면 지우기
                // SEND   clear:<EndOfCommand>
                // RETURN -
                ClearPgm();

                processRemoteReturn = "clear:<EndOfCommand>";

            } else if (cmd.Equals("free:")) {
                // 자유송출
                // SEND   free:str<EndOfCommand>
                // RETURN -
                TextBoxFreeContent.Text = param;
                FreeCut();

                processRemoteReturn = "free:<EndOfCommand>";

            } else if (cmd.Equals("update")) {
                // 페이지 리스트 동기화
                // SEND   update:<EndOfCommand>
                // RETURN string pageList

                processRemoteReturn = "update:" + PackPageListToString() + "<EndOfCommand>";

            } else if (cmd.Equals("open")) {
                // 다른 파일 열기 또는 현재 파일 다시 불러오기
                // SEND   open:path<EndOfCommand>
                // RETURN string pageList
                if (param.Equals("")) {
                    // 현재 파일 다시 불러오기
                    OpenTxtFile(false);
                } else {
                    // 다른 파일 열기
                    fileName = fileNameTrimmer.Replace(param, "$2");
                    filePath = fileNameTrimmer.Replace(param, "$1");
                    OpenTxtFile(true);
                }

                processRemoteReturn = "open:" + PackPageListToString() + "<EndOfCommand>";

            } else if (cmd.Equals("ls")) {
                // 파일 목록 요청
                // SEND   ls:<EndOfCommand>
                // RETURN string fileList
                processRemoteReturn = "ls:" /*+ "FILE LIST SEPERATED BY LF" */+ "<EndOfCommand>";

            } else if (cmd.Equals("presenter")) {
                // 송출 창 토글
                // SEND   presenter:<EndOfCommand>
                // RETURN bool isOpen (창 열었으면 "true")
                processRemoteReturn = "presenter:" + OpenAndClosePresenterWindow().ToString().ToLower() + "<EndOfCommand>";

            } else if (cmd.Equals("terminate")) {
                // 서버 종료
                // SEND   terminate:<EndOfCommand>
                // RETURN -
                MenuItemRemote.IsChecked = false;
                processRemoteReturn = "terminate:<EndOfCommand>";

            } else {
                // 정의되지 않은 명령
                processRemoteReturn = cmd + ":<EndOfCommand>";
            }
        }

        public string PreProcessRemote(string cmd, string param) {
            // https://stackoverflow.com/questions/19009174/dispatcher-invoke-vs-begininvoke-confusion
            if (Dispatcher.CheckAccess()) {
                ProcessRemote(cmd, param);
            } else {
                Dispatcher.Invoke(
                    new Action(() => ProcessRemote(cmd, param)),
                    System.Windows.Threading.DispatcherPriority.Normal,
                    null);
            }
            return processRemoteReturn;
        }

        private void MenuItemRemote_Click(object sender, RoutedEventArgs e) {
            // https://msdn.microsoft.com/ko-kr/library/7a2f3ay4(v=vs.90).aspx
            if (MenuItemRemote.IsChecked) {
                //MenuItemRemote.IsEnabled = false;
                threadRemote = new Thread(SynSocketListener.StartListening);
                threadRemote.IsBackground = true;
                threadRemote.Start();
            } else {
                SynSocketListener.TerminateListening();
            }
        }

    }
}
