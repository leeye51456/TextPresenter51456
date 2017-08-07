using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    /// HelpWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HelpWindow : Window {

        public HelpWindow(int tabNum) {
            InitializeComponent();

            TabControlBody.SelectedIndex = tabNum;
            TextBlockVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void ButtonCheckVersion_Click(object sender, RoutedEventArgs e) {
            ButtonCheckVersion.Content = "최신 버전 확인 중...";
            try {
                WebClient client = new WebClient();
                string reply = client.DownloadString(@"https://raw.githubusercontent.com/leeye51456/TextPresenter51456/master/TextPresenter51456/VersionInfo.txt");
                ButtonCheckVersion.Content = "최신 버전 " + reply;
            } catch (Exception ex) {
                ButtonCheckVersion.Content = "최신 버전 확인 실패";
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
