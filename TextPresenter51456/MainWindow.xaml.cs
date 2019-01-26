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
        List<bool> isTitleList;
        string freePresentationText;
        int freePresentationLine = 3;
        PageNumberLog pageNum = new PageNumberLog();
        PageNumberManager pvwManager = new PageNumberManager(1, 1, false);
        PageNumberManager pgmManager = new PageNumberManager(0, 0, true);

        Regex newLineUnifier = new Regex(@"(\r\n|\r)");
        Regex pageBreaker = new Regex(@"\n{2,}");
        Regex sharp = new Regex(@"^#+\s*");
        Regex sharpEscaped = new Regex(@"^\\#(#*\s*)");
        Regex partialLineComment = new Regex(@"[ \t]*\/\/.*?(\n|$)"); // -> $1
        Regex firstFullLineComment = new Regex(@"^[ \t]*\/\/.*?\n");
        Regex middleFullLineComment = new Regex(@"\n[ \t]*\/\/.*?\n");
        Regex lastFullLineComment = new Regex(@"\n[ \t]*\/\/.*?$");
        Regex pageComment = new Regex(@"^([ \t]*\/\/.*?\n?)+$");
        Regex commentEscaped = new Regex(@"(\\\/\/|\/\\\/|\\\/\\\/)"); // \//, /\/, \/\/
        Regex trimmer = new Regex(@"(^\n+|\n+$)");
        Regex fileNameTrimmer = new Regex(@"^(.*\\)(.*?)$");

        SolidColorBrush DEFAULT_BORDER_COLOR = MiscConverter.IntToSolidColorBrush(0xe0e0e0);
        SolidColorBrush PVW_BORDER_COLOR = MiscConverter.IntToSolidColorBrush(0x00a000);
        SolidColorBrush PGM_BORDER_COLOR = MiscConverter.IntToSolidColorBrush(0xc00000);
        SolidColorBrush titleColor;

        PresenterWindow pw;
        SettingWindow sw;
        HelpWindow hw;

        Thread threadRemote;
        string processRemoteReturn;


        public void GetTitleColor() {
            try {
                titleColor = MiscConverter.IntToSolidColorBrush(MiscConverter.StringToIntColor(Setting.GetAttribute("titleColor")));
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                titleColor = MiscConverter.IntToSolidColorBrush(0xffff00);
            }

            if (freePresentationText != null) {
                // free text presentation: don't change contents
                UpdateFree();
            }
            if (WrapPanelPageList != null && WrapPanelPageList.Children.Count > 1) {
                // if a text file is open, the minimum of Count is 2
                int len = textList.Count;
                for (int i = 1; i < len; i++) {
                    if (isTitleList[i]) {
                        (WrapPanelPageList.Children[i - 1] as Button).Foreground = titleColor;
                    }
                }
            }

            if (isTitleList != null) {
                if (PgmContent != null && isTitleList[pgmManager.PageNumber]) {
                    PgmContent.Foreground = titleColor;
                    if (pw != null) {
                        pw.LabelPresenterText.Foreground = titleColor;
                    }
                }

                if (PvwContent != null && isTitleList[pvwManager.PageNumber]) {
                    PvwContent.Foreground = titleColor;
                }
            }
        }

        public MainWindow() {
            SynSocketListener.mw = this;

            Setting.Load();
            GetTitleColor();

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
            // PresenterWindow is open
            if (pw != null) {
                switch (MessageBox.Show("송출 창이 열려 있습니다. 정말로 종료하시겠습니까?", "TextPresenter51456", MessageBoxButton.YesNo)) {
                    case MessageBoxResult.Yes:
                        // check one more time: the case is possible of closing PresenterWindow before click "Yes"
                        if (pw != null) {
                            pw.Close();
                        }
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void UpdateBorders() {
            if (WrapPanelPageList.Children.Count <= 1) {
                // if a text file is open, the minimum of Count is 2.
                // in this case, no text file is open.
                return;
            }

            // for update order is pvw -> pgm, border color should be updated during UpdatePvw
            foreach (Button item in WrapPanelPageList.Children) {
                item.BorderBrush = DEFAULT_BORDER_COLOR;
            }
            // for page list is 0-based index, number should be -1
            Button pvwBtn = WrapPanelPageList.Children[pvwManager.PageNumber - 1] as Button;
            pvwBtn.BorderBrush = PVW_BORDER_COLOR;
            if (pgmManager.PageNumber > 0) {
                Button pgmBtn = WrapPanelPageList.Children[pgmManager.PageNumber - 1] as Button;
                pgmBtn.BorderBrush = PGM_BORDER_COLOR;
            }
        }
        private void ClearPgm() {
            CheckBoxFreeLive.IsChecked = false;
            pgmManager.PageNumber = 0;
            UpdateBorders();
            freePresentationText = null;
            PgmContent.Content = "";
            PgmPage.Content = "-";
            // PresenterWindow is open
            if (pw != null) {
                pw.LabelPresenterText.Content = "";
            }
        }
        private void UpdatePgm() {
            if (textList == null || pgmManager.PageNumber == 0) {
                return;
            }
            CheckBoxFreeLive.IsChecked = false;
            freePresentationText = null;
            SolidColorBrush fg = isTitleList[pgmManager.PageNumber] ? titleColor : Brushes.White;
            PgmContent.Content = textList[pgmManager.PageNumber];
            PgmContent.Foreground = fg;
            PgmPage.Content = pgmManager.PageNumber;
            // PresenterWindow is open
            if (pw != null) {
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
            PvwContent.Foreground = isTitleList[pvwManager.PageNumber] ? titleColor : Brushes.White;
            PvwPage.Content = pvwManager.PageNumber;
        }

        private void OpenTxtFile(bool resetPvw) {
            Encoding encoding;
            if (resetPvw) {
                pvwManager.PageNumber = 1;
            }
            if (!int.TryParse(Setting.GetAttribute("textEncoding"), out int textEncoding) || textEncoding == 0) {
                textEncoding = 0;
                encoding = Encoding.Default;
            } else {
                try {
                    encoding = Encoding.GetEncoding(textEncoding);
                } catch (Exception ex) {
                    MessageBox.Show(
                        ex.Message + "\n프로그램의 인코딩 설정(" + textEncoding.ToString() + ")이 잘못되었습니다.",
                        "TextPresenter51456");
                    textEncoding = 0;
                    encoding = Encoding.Default;
                }
            }
            // read the selected file
            try {
                System.IO.StreamReader sr = new System.IO.StreamReader(filePath + fileName, encoding);
                string fullText = sr.ReadToEnd();
                sr.Close();
                fullText = newLineUnifier.Replace(fullText, "\n"); // unify newline characters
                fullText = trimmer.Replace(fullText, ""); // trimming newlines
                textList = new List<string>(pageBreaker.Split(fullText)); // split pages
                textList.Insert(0, ""); // leave index 0 empty for convenience
                textList.Add(""); // add an empty page at the end
                WindowMainWindow.Title = "TextPresenter51456 - " + fileName + " (" + filePath + ")";
            } catch (Exception e) {
                string errMsg = "Fail to read the file";
                errMsg += "\nMessage: " + e.Message;
                errMsg += "\nSource" + e.Source;
                MessageBox.Show(errMsg, "TextPresenter51456");
                return;
            }
            // show the file read as the page list
            try {
                int len = textList.Count;
                int i = 1;
                Style pageListItemStyle = FindResource("PageListItem") as Style;

                isTitleList = new List<bool>(len) {
                    false
                };

                WrapPanelPageList.Children.Clear();

                // construct page list
                while (i < len) {
                    // unify escaped comments
                    if (commentEscaped.IsMatch(textList[i])) {
                        textList[i] = commentEscaped.Replace(textList[i], @"/\/");
                    }
                    // remove comments
                    if (pageComment.IsMatch(textList[i])) { // page comment -> remove the page
                        textList.RemoveAt(i);
                        textList.Capacity -= 1;
                        len -= 1;
                        continue;
                    }
                    if (middleFullLineComment.IsMatch(textList[i])) { // full line comment (the middle line of the page) -> remove the line
                        textList[i] = middleFullLineComment.Replace(textList[i], "\n");
                    }
                    if (firstFullLineComment.IsMatch(textList[i])) { // full line comment (the 1st line of the page) -> remove the line
                        textList[i] = firstFullLineComment.Replace(textList[i], "");
                    }
                    if (lastFullLineComment.IsMatch(textList[i])) { // full line comment (the last line of the page) -> remove the line
                        textList[i] = lastFullLineComment.Replace(textList[i], "");
                    }
                    if (partialLineComment.IsMatch(textList[i])) { // partial line comment -> remove the comment
                        textList[i] = partialLineComment.Replace(textList[i], "$1");
                    }
                    // correct escaped comments
                    if (commentEscaped.IsMatch(textList[i])) {
                        textList[i] = commentEscaped.Replace(textList[i], "//");
                    }

                    Button pageListItem = new Button {
                        Style = pageListItemStyle,
                        Tag = i
                    };
                    if (sharp.IsMatch(textList[i])) { // remove beginning #s and make these pages "header pages"
                        textList[i] = sharp.Replace(textList[i], "");
                        isTitleList.Add(true);
                        pageListItem.Foreground = titleColor;
                    } else {
                        isTitleList.Add(false);
                    }
                    if (sharpEscaped.IsMatch(textList[i])) { // escaped # (\#)
                        textList[i] = sharpEscaped.Replace(textList[i], "#$1");
                    }
                    pageListItem.Content = textList[i];

                    WrapPanelPageList.Children.Add(pageListItem);

                    i++;
                }
                pgmManager.LastPageNumber = pvwManager.LastPageNumber = len - 1;
                LabelPageIndicator.Content = "/" + (len - 1).ToString();
                UpdatePvw();
            } catch (Exception e) {
                string errMsg = "Failed to display the file";
                errMsg += "\nMessage: " + e.Message;
                errMsg += "\nSource" + e.Source;
                MessageBox.Show(errMsg, "TextPresenter51456");
            }

            MenuItemRefresh.IsEnabled = true;
        }

        private bool SetFileNameAndPath(string src) {
            // 1. txt file -> pass
            // 2. non-txt file & Yes -> pass
            // 3. non-txt file & No -> cancel
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
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog {
                Multiselect = false,
                DefaultExt = ".txt",
                Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*"
            };

            if (ofd.ShowDialog() == true) {
                // if canceled
                if (!SetFileNameAndPath(ofd.FileName)) {
                    return;
                }
                OpenTxtFile(true);
            }
        }

        private void UpdateFree() {
            SolidColorBrush fg;
            string text = freePresentationText;

            if (sharp.IsMatch(text)) { // remove beginning #s and make this page "header page"
                text = sharp.Replace(text, "");
                fg = titleColor;
            } else {
                fg = Brushes.White;
            }
            if (sharpEscaped.IsMatch(text)) { // escaped # (\#)
                text = sharpEscaped.Replace(text, "#$1");
            }

            PgmContent.Content = text;
            PgmContent.Foreground = fg;
            // PresenterWindow is open
            if (pw != null) {
                pw.LabelPresenterText.Content = text;
                pw.LabelPresenterText.Foreground = fg;
            }
        }
        private void FreeCut() {
            string[] freeTextSplit;
            pgmManager.PageNumber = 0;
            PgmPage.Content = "자유송출";
            UpdateBorders();
            freePresentationText = newLineUnifier.Replace(TextBoxFreeContent.Text, "\n"); //freePresentationLine
            freeTextSplit = freePresentationText.Split('\n');
            freePresentationText = "";
            for (int i = 0, last = freeTextSplit.Count() - 1; i < freePresentationLine && last - i >= 0; i++) {
                freePresentationText = freeTextSplit[last - i] + "\n" + freePresentationText;
            }
            UpdateFree();
        }

        // files are dragged into MainWindow
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

        // open file command (open file menu)
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            SelectTextFile();
        }

        // refresh the page list (reopen menu)
        private void MenuItemRefresh_Click(object sender, RoutedEventArgs e) {
            OpenTxtFile(false);
        }

        // exit menu
        private void MenuItemExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        // when PresenterWindow is unloaded, make pw null to check easily whether PresenterWindow is open
        private void OnPwUnloaded(object sender, RoutedEventArgs e) {
            pw = null;
        }
        private void OpenPresenterWindow() {
            if (pw == null) {
                // PresenterWindow is not open
                pw = new PresenterWindow(this);
                pw.Show();
                // event handler management (ko): https://msdn.microsoft.com/ko-kr/library/ms742806(v=vs.110).aspx
                pw.AddHandler(UnloadedEvent, new RoutedEventHandler(OnPwUnloaded));
                UpdatePgm();
            } else {
                MessageBox.Show("이미 송출 창이 열려 있습니다.\n이 메시지를 보고 있다면 개발자에게 알리십시오.", "TextPresenter51456");
            }
        }
        private void ClosePresenterWindow() {
            if (pw != null) {
                // PresenterWindow is already open
                pw.Close();
                RoutedEventArgs rea = new RoutedEventArgs(UnloadedEvent) {
                    Source = pw
                };
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
                // PresenterWindow is not open
                ButtonPresenterWindow.IsChecked = true; // Checked event will be occured
                return true;
            } else {
                // PresenterWindow is already open
                ButtonPresenterWindow.IsChecked = false; // Unhecked event will be occured
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
                    // any number in pageNum
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

        private void CheckFreeLine() {
            if (!int.TryParse(TextBoxFreeLines.Text, out freePresentationLine)) {
                MessageBox.Show("양의 정수만 입력 가능합니다.",
                    "TextPresenter51456",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (freePresentationText != null) {
                FreeCut();
            }
        }

        private void ButtonApplyFreeLine_Click(object sender, RoutedEventArgs e) {
            CheckFreeLine();
        }

        private void TextBoxFreeContent_KeyUp(object sender, KeyEventArgs e) {
            if (CheckBoxFreeLive.IsChecked == true) {
                FreeCut();
            }
        }

        private void WindowMainWindow_PreviewKeyDown(object sender, KeyEventArgs e) { // process input when keydown
            if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.Escape) { // Shift+ESC: presenter window button
                OpenAndClosePresenterWindow();
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter) { // Ctrl+Enter: free text presentation
                FreeCut();
                return;
            }
            if (MenuItemRefresh.IsEnabled == true && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.R) { // Ctrl+R: reload the current file
                OpenTxtFile(false);
                return;
            }

            if (TextBoxFreeContent.IsFocused) { // free text presentation box is focused
                switch (e.Key) {
                case Key.Escape:
                    KillFocus();
                    break;
                case Key.OemPeriod: // normal .(>) key
                case Key.Decimal: // keypad .(Del) key
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                        ClearPgm();
                    }
                    break;
                }
            } else if (TextBoxFreeLines.IsFocused) { // free text presentation line limit box is focused
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) {
                    CheckFreeLine();
                }
            } else {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) { // + Shift
                    switch (e.Key) {
                        case Key.Up:
                            ScrollViewerPageList.ScrollToVerticalOffset(ScrollViewerPageList.VerticalOffset - 130);
                            e.Handled = true;
                            break;
                        case Key.Down:
                            ScrollViewerPageList.ScrollToVerticalOffset(ScrollViewerPageList.VerticalOffset + 130);
                            e.Handled = true;
                            break;
                    }
                } else { // no modifier
                    if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) { // numpad
                        pageNum.Add(e.Key - Key.NumPad0);
                        UpdatePageIndicator();
                        return;
                    }
                    if (e.Key >= Key.D0 && e.Key <= Key.D9) { // number keys above Q-P
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
                            if (WrapPanelPageList != null && WrapPanelPageList.Children.Count > 1) {
                                pvwManager.PageNumber -= (int)(WrapPanelPageList.ActualWidth / ((WrapPanelPageList.Children[0] as Button).ActualWidth + 4));
                                UpdatePvw();
                            }
                            KillFocus();
                            e.Handled = true;
                            break;
                        case Key.Down:
                            if (WrapPanelPageList != null && WrapPanelPageList.Children.Count > 1) {
                                pvwManager.PageNumber += (int)(WrapPanelPageList.ActualWidth / ((WrapPanelPageList.Children[0] as Button).ActualWidth + 4));
                                UpdatePvw();
                            }
                            KillFocus();
                            e.Handled = true;
                            break;
                        case Key.OemPeriod: // normal  .(>) key
                        case Key.Decimal: // keypad .(Del) key
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
        }

        private void ScrollViewerPageList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            // file is not loaded yet
            if (fileName == null) {
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
            sw = new SettingWindow(this, pw) {
                Owner = this
            };
            sw.Show();
            if (pw != null) {
                pw.IsEnabled = false;
            }
            IsEnabled = false;
        }
        private void MenuItemReloadSettings_Click(object sender, RoutedEventArgs e) {
            Setting.Load();
            GetTitleColor();
            if (pw != null) {
                pw.ApplySettings();
            }
        }

        private void MenuItemHelp_Click(object sender, RoutedEventArgs e) {
            hw = new HelpWindow(this, pw) {
                Owner = this
            };
            hw.Show();
            if (pw != null) {
                pw.IsEnabled = false;
            }
            IsEnabled = false;
        }


        private string PackPageListToString() {
            string result = "";
            /*
             *  int isTitleList[i]
             *  string textList[i]
             *  
             *  int isTitleList[i + 1]
             *  string textList[i + 1]
             *  
             *  ...
             */
            if (textList.Count < 1) {
                return result;
            }
            result += isTitleList[1].ToString() + "\n" + textList[1];
            for (int i = 2; i < textList.Count; i++) {
                result += "\n\n" + isTitleList[i].ToString() + "\n" + textList[i];
            }
            return result;
        }
        private void ProcessRemote(string cmd, string param) {
            processRemoteReturn = "";

            if (cmd.Equals("pgm")) {
                // change PGM
                // SEND   pgm:page<EndOfCommand>
                // RETURN -
                if (int.TryParse(param, out int pageNum)) {
                    // if valid
                    pgmManager.PageNumber = pageNum;
                    pvwManager.PageNumber = pageNum + 1;
                    UpdatePgm();
                    UpdatePvw();
                }

                processRemoteReturn = "pgm:" + pgmManager.PageNumber + "<EndOfCommand>";

            } else if (cmd.Equals("pvw")) {
                // change PVW
                // SEND   pvw:page<EndOfCommand>
                // RETURN -
                if (int.TryParse(param, out int pageNum)) {
                    // if valid
                    pvwManager.PageNumber = pageNum;
                    UpdatePvw();
                }

                processRemoteReturn = "pvw:" + pvwManager.PageNumber + "<EndOfCommand>";

            } else if (cmd.Equals("cut")) {
                // cut
                // SEND   cut:<EndOfCommand>
                // RETURN -
                CutAction();

                processRemoteReturn = "cut:" + pgmManager.PageNumber + "<EndOfCommand>";

            } else if (cmd.Equals("clear")) {
                // clear presenter window screen
                // SEND   clear:<EndOfCommand>
                // RETURN -
                ClearPgm();

                processRemoteReturn = "clear:<EndOfCommand>";

            } else if (cmd.Equals("free:")) {
                // free text presentation
                // SEND   free:str<EndOfCommand>
                // RETURN -
                TextBoxFreeContent.Text = param;
                FreeCut();

                processRemoteReturn = "free:<EndOfCommand>";

            } else if (cmd.Equals("update")) {
                // page list synchronization
                // SEND   update:<EndOfCommand>
                // RETURN string pageList

                processRemoteReturn = "update:" + PackPageListToString() + "<EndOfCommand>";

            } else if (cmd.Equals("open")) {
                // open the other file or reload current file
                // SEND   open:path<EndOfCommand>
                // RETURN string pageList
                if (param.Equals("")) {
                    // reload current file
                    OpenTxtFile(false);
                } else {
                    // open the other file
                    fileName = fileNameTrimmer.Replace(param, "$2");
                    filePath = fileNameTrimmer.Replace(param, "$1");
                    OpenTxtFile(true);
                }

                processRemoteReturn = "open:" + PackPageListToString() + "<EndOfCommand>";

            } else if (cmd.Equals("ls")) {
                // request the file list
                // SEND   ls:<EndOfCommand>
                // RETURN string fileList
                processRemoteReturn = "ls:" /*+ "FILE LIST SEPERATED BY LF" */+ "<EndOfCommand>";

            } else if (cmd.Equals("presenter")) {
                // toggle the presenter window
                // SEND   presenter:<EndOfCommand>
                // RETURN bool isOpen (창 열었으면 "true")
                processRemoteReturn = "presenter:" + OpenAndClosePresenterWindow().ToString().ToLower() + "<EndOfCommand>";

            } else if (cmd.Equals("terminate")) {
                // terminate server
                // SEND   terminate:<EndOfCommand>
                // RETURN -
                MenuItemRemote.IsChecked = false;
                processRemoteReturn = "terminate:<EndOfCommand>";

            } else {
                // undefined command
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
                threadRemote = new Thread(SynSocketListener.StartListening) {
                    IsBackground = true
                };
                threadRemote.Start();
            } else {
                SynSocketListener.TerminateListening();
            }
        }

    }
}
