using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

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
            LocUtil.SetDefaultLanguage(this);

            // Adjust checked language menu item
            foreach (MenuItem item in menuItemLanguages.Items)
            {
                if (item.Tag.ToString().Equals(LocUtil.GetCurrentCultureName(this)))
                    item.IsChecked = true;
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem item in menuItemLanguages.Items)
            {
                item.IsChecked = false;
            }

            MenuItem mi = sender as MenuItem; //Console.WriteLine("menu tag: " + mi.Tag.ToString());
            mi.IsChecked = true;
            //SwitchLanguage(mi.Tag.ToString());
            LocUtil.SwitchLanguage(this, mi.Tag.ToString());
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
            catch (Exception ex)
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
        public void Uninstall()
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
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            labelspeed.Text = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        public void enableBtn()
        {
            btnStartDownload.IsEnabled = true;
            btn3.IsEnabled = true;
            btn4.IsEnabled = true;
            btn1.IsEnabled = true;
        }
        public void disableBtn()
        {
            btnStartDownload.IsEnabled = false;
            btn1.IsEnabled = false;
            btn3.IsEnabled = false;
            btn4.IsEnabled = false;
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download Completed, Pleaase press OK and continue on installation window.", "Download Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            input_text.Text = "Download Completed";
            labelspeed.Text = null;
            enableBtn();
            Process.Start(path + "temp.exe");
            sw.Reset();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            KillBlitz();
            System.Threading.Thread.Sleep(2000);
            Uninstall();
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            // Starts the download
            sw.Start();
            client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), path + "temp.exe");
            input_text.Text = "Downloading Blitz.exe";
            disableBtn();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            // Starts the download
            sw.Start();
            client.DownloadFileAsync(new Uri("https://aka.ms/vs/16/release/vc_redist.x86.exe"), path + "temp.exe");
            input_text.Text = "Downloading vc_redist.x86.exe";
            disableBtn();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Cache successfully cleared");
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            Uninstall();
            MessageBox.Show("Blitz has been uninstalled", "Blitz Uninstallation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        //Removes Champions folder
        private void Button_Click_4(object sender, RoutedEventArgs e)
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
    }
}
