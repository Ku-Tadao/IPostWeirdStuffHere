using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace Blitz_Troubleshooter_V2._3
{
    /// <summary>
    /// Interaction logic for English.xaml
    /// </summary>
    public partial class English : Window
    {
        public English()
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
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            System.Threading.Thread.Sleep(1000);
            Uninstall();
            MessageBox.Show("Blitz has been uninstalled", "Blitz Uninstallation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}