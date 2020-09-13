using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace Blitz_Troubleshooter_V2._3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public string[] paths = new String[] {
        localappdata + "\\blitz-updater",
        localappdata + "\\Programs\\blitz-core",
        appdata + "\\Blitz",
        appdata + "\\blitz-core",
        appdata + "\\Blitz-helpers",
        localappdata + "\\Programs\\Blitz"
        };
        public string path = Path.GetTempPath();
        //Is league client running
        public static bool IsRunning(string processName)
        {
            try
            {
                var retVal = Process.GetProcesses().Any(p => p.ProcessName.Contains(processName));
                return retVal;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //Get league clinet path
        public static string getClientPath(string processName)
        {
            List<string> mainPaths = new List<string>();
            Process[] leagueClient = Process.GetProcessesByName(processName);
            string leaguePath = leagueClient[0].MainModule.FileName;
            string[] pathStrings = leaguePath.Split('\\');

            for (var i = 0; i < pathStrings.Length - 1; i++)
            {
                mainPaths.Add(pathStrings[i]);
            }
            var mainPath = string.Join<string>("\\", mainPaths);
            return mainPath;
        }

        public void KillBlitz()
        {
            foreach (Process proc in Process.GetProcessesByName("Blitz"))
            {
                proc.Kill();
            }

        }
        public void AppdataBlitz()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz";
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        public void ENUninstall()
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }
        Stopwatch sw = new Stopwatch();
        void ENclient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            ENlabelspeed.Text = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            ENprogressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        public void ENenableBtn()
        {
            ENbtnStartDownload.IsEnabled = true;
            ENbtn3.IsEnabled = true;
            ENbtn4.IsEnabled = true;
            ENbtn1.IsEnabled = true;
        }
        public void ENdisableBtn()
        {
            ENbtnStartDownload.IsEnabled = false;
            ENbtn1.IsEnabled = false;
            ENbtn3.IsEnabled = false;
            ENbtn4.IsEnabled = false;
        }
        void ENclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download Completed, Pleaase press OK and continue on installation window.", "Download Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            ENinput_text.Text = "Download Completed";
            ENlabelspeed.Text = null;
            ENenableBtn();
            Process.Start(path + "temp.exe");
            sw.Reset();
        }
        private void ENButton_Click(object sender, RoutedEventArgs e)
        {

            KillBlitz();
            System.Threading.Thread.Sleep(2000);
            ENUninstall();
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ENclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(ENclient_DownloadFileCompleted);

            // Starts the download
            sw.Start();
            client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), path + "temp.exe");
            ENinput_text.Text = "Downloading Blitz.exe";
            ENdisableBtn();
        }
        private void ENButton_Click_1(object sender, RoutedEventArgs e)
        {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ENclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(ENclient_DownloadFileCompleted);

            // Starts the download
            sw.Start();
            client.DownloadFileAsync(new Uri("https://aka.ms/vs/16/release/vc_redist.x86.exe"), path + "temp.exe");
            ENinput_text.Text = "Downloading vc_redist.x86.exe";
            ENdisableBtn();
        }
        private void ENButton_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Cache successfully cleared");
        }
        private void ENButton_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            ENUninstall();
            MessageBox.Show("Blitz has been uninstalled", "Blitz Uninstallation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        //Removes Champions folder
        private void ENButton_Click_3(object sender, RoutedEventArgs e)
        {
            if (IsRunning("LeagueClient"))
            {
                var path = getClientPath("LeagueClient") + @"\Config\Champions";
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                MessageBox.Show("All Build has been cleared", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("League Client is not running. You need to have league client running in order to clear builds.", "League Client", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void DEUninstall()
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            MessageBox.Show("Blitz wurde deinstalliert ", " Blitz-Deinstallation ", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        void DEclient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            DEprogressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        void DEclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download abgeschlossen");
            DEinput_text.Text = "Warten auf Eingabe";
            DEbtnStartDownload.IsEnabled = true;
            DEbtn3.IsEnabled = true;
            DEbtn4.IsEnabled = true;
            DEbtn1.IsEnabled = true;
            Process.Start(path + "temp.exe");
        }
        private void DEButton_Click(object sender, RoutedEventArgs e)
        {

            KillBlitz();
            DEUninstall();
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DEclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(DEclient_DownloadFileCompleted);

            // Starts the download
            client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), path + "temp.exe");
            DEinput_text.Text = "Blitz.exe wird heruntergeladen";
            DEbtnStartDownload.IsEnabled = false;
            DEbtn1.IsEnabled = false;
            DEbtn3.IsEnabled = false;
            DEbtn4.IsEnabled = false;
        }
        private void DEButton_Click_1(object sender, RoutedEventArgs e)
        {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DEclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(DEclient_DownloadFileCompleted);

            // Starts the download
            client.DownloadFileAsync(new Uri("https://aka.ms/vs/16/release/vc_redist.x86.exe"), path + "temp.exe");
            DEinput_text.Text = "vc_redist.x86.exe wird heruntergeladen";
            DEbtnStartDownload.IsEnabled = false;
            DEbtn1.IsEnabled = false;
            DEbtn3.IsEnabled = false;
            DEbtn4.IsEnabled = false;
        }
        private void DEButton_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Cache erfolgreich geleert");
        }
        private void DEButton_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            DEUninstall();
        }

        public void PLUninstall()
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            MessageBox.Show("Blitz został odinstalowany", " odinstalowywanie Blitza", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        void PLclient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            PLprogressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        void PLclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Pobieranie zakończone");
            PLinput_text.Text = "Czekam na wejście";
            PLbtnStartDownload.IsEnabled = true;
            PLbtn3.IsEnabled = true;
            PLbtn4.IsEnabled = true;
            PLbtn1.IsEnabled = true;
            Process.Start(path + "temp.exe");
        }
        private void PLButton_Click(object sender, RoutedEventArgs e)
        {

            KillBlitz();
            PLUninstall();
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(PLclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(PLclient_DownloadFileCompleted);

            // Starts the download
            client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), path + "temp.exe");
            PLinput_text.Text = "Pobranie pliku Blitz.exe";
            PLbtnStartDownload.IsEnabled = false;
            PLbtn1.IsEnabled = false;
            PLbtn3.IsEnabled = false;
            PLbtn4.IsEnabled = false;
        }
        private void PLButton_Click_1(object sender, RoutedEventArgs e)
        {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(PLclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(PLclient_DownloadFileCompleted);

            // Starts the download
            client.DownloadFileAsync(new Uri("https://aka.ms/vs/16/release/vc_redist.x86.exe"), path + "temp.exe");
            PLinput_text.Text = "ZostaniePobieraine pliku vc_redist.x86.exe";
            PLbtnStartDownload.IsEnabled = false;
            PLbtn1.IsEnabled = false;
            PLbtn3.IsEnabled = false;
            PLbtn4.IsEnabled = false;
        }
        private void PLButton_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Pamięć podręczna została pomyślnie wyczyszczona");
        }
        private void PLButton_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            PLUninstall();
        }

        public void TRUninstall()
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            MessageBox.Show("Blitz kaldırıldı ", " Blitz Kaldırma ", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        void TRclient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            TRprogressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        void TRclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("İndirme tamamlandı");
            TRinput_text.Text = "Giriş bekleniyor";
            TRbtnStartDownload.IsEnabled = true;
            TRbtn3.IsEnabled = true;
            TRbtn4.IsEnabled = true;
            TRbtn1.IsEnabled = true;
            Process.Start(path + "temp.exe");
        }
        private void TRButton_Click(object sender, RoutedEventArgs e)
        {

            KillBlitz();
            TRUninstall();
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(TRclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(TRclient_DownloadFileCompleted);

            // Starts the download
            client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), path + "temp.exe");
            TRinput_text.Text = "Blitz.exe indiriliyor";
            TRbtnStartDownload.IsEnabled = false;
            TRbtn1.IsEnabled = false;
            TRbtn3.IsEnabled = false;
            TRbtn4.IsEnabled = false;
        }
        private void TRButton_Click_1(object sender, RoutedEventArgs e)
        {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(TRclient_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(TRclient_DownloadFileCompleted);

            // Starts the download
            client.DownloadFileAsync(new Uri("https://aka.ms/vs/16/release/vc_redist.x86.exe"), path + "temp.exe");
            TRinput_text.Text = "Vc_redist.x86.exe indiriliyor";
            TRbtnStartDownload.IsEnabled = false;
            TRbtn1.IsEnabled = false;
            TRbtn3.IsEnabled = false;
            TRbtn4.IsEnabled = false;
        }
        private void TRButton_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Önbellek başarıyla temizlendi");
        }
        private void TRButton_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            TRUninstall();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Welcome to my Unofficial Troubleshooter for the Blitz application, if you've pressed this button it means you've probably been in the need of some information regarding the program." + "\n\n" + "There are currently 4 options to choose from in order to solve your issues" + "\n\n" + "If you'd like some detailed information regarding these options, please message me on Discord:" + "\n" + "Kubi#2468", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void cblang_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            if (cblang.SelectedValue.ToString() == "English")
            {
                StpLanguage.HorizontalAlignment = HorizontalAlignment.Right;
                StpLanguage.VerticalAlignment = VerticalAlignment.Top;

                GridEnglish.Visibility = Visibility.Visible;
                GridGerman.Visibility = Visibility.Collapsed;
                GridPolish.Visibility = Visibility.Collapsed;
                GridTurkish.Visibility = Visibility.Collapsed;

            }
            else if (cblang.SelectedValue.ToString() == "German")
            {
                StpLanguage.HorizontalAlignment = HorizontalAlignment.Right;
                StpLanguage.VerticalAlignment = VerticalAlignment.Top;

                GridEnglish.Visibility = Visibility.Collapsed;
                GridGerman.Visibility = Visibility.Visible;
                GridPolish.Visibility = Visibility.Collapsed;
                GridTurkish.Visibility = Visibility.Collapsed;
            }
            else if (cblang.SelectedValue.ToString() == "Turkish")
            {
                StpLanguage.HorizontalAlignment = HorizontalAlignment.Right;
                StpLanguage.VerticalAlignment = VerticalAlignment.Top;

                GridEnglish.Visibility = Visibility.Collapsed;
                GridGerman.Visibility = Visibility.Collapsed;
                GridPolish.Visibility = Visibility.Collapsed;
                GridTurkish.Visibility = Visibility.Visible;
            }
            else if (cblang.SelectedValue.ToString() == "Polish")
            {
                StpLanguage.HorizontalAlignment = HorizontalAlignment.Right;
                StpLanguage.VerticalAlignment = VerticalAlignment.Top;

                GridEnglish.Visibility = Visibility.Collapsed;
                GridGerman.Visibility = Visibility.Collapsed;
                GridPolish.Visibility = Visibility.Visible;
                GridTurkish.Visibility = Visibility.Collapsed;
            }

        }
    }
}
