using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace Blitz_Troubleshooter_V2._3
{
    /// <summary>
    ///     Interaction logic for English.xaml
    /// </summary>
    public partial class English
    {
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string Localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private readonly string _path = Path.GetTempPath();

        private readonly string[] _paths =
        {
            Localappdata + "\\blitz-updater",
            Localappdata + "\\Programs\\blitz-core",
            Appdata + "\\Blitz",
            Appdata + "\\blitz-core",
            Appdata + "\\Blitz-helpers",
            Localappdata + "\\Programs\\Blitz"
        };

        private readonly Stopwatch _sw = new Stopwatch();

        public English()
        {
            InitializeComponent();
        }

        private static void KillBlitz()
        {
            foreach (var proc in Process.GetProcessesByName("Blitz")) proc.Kill();
        }

        private static void AppdataBlitz()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz";
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        private void Uninstall()
        {
            foreach (var path in _paths)
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytesIn = double.Parse(e.BytesReceived.ToString());
            var totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            var percentage = bytesIn / totalBytes * 100;
            Labelspeed.Text = $"{(e.BytesReceived / 1024d / _sw.Elapsed.TotalSeconds):0.00} kb/s";
            ProgressBar1.Value = int.Parse(Math.Truncate(percentage).ToString(CultureInfo.InvariantCulture));
        }

        private void EnableBtn()
        {
            BtnStartDownload.IsEnabled = true;
            Btn3.IsEnabled = true;
            Btn4.IsEnabled = true;
            Btn1.IsEnabled = true;
        }

        private void DisableBtn()
        {
            BtnStartDownload.IsEnabled = false;
            Btn1.IsEnabled = false;
            Btn3.IsEnabled = false;
            Btn4.IsEnabled = false;
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download Completed, Please press OK and continue on installation window.",
                "Download Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            InputText.Text = "Download Completed";
            Labelspeed.Text = null;
            EnableBtn();
            Process.Start(_path + "temp.exe");
            _sw.Reset();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            Thread.Sleep(2000);
            Uninstall();
            var client = new WebClient();
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;

            // Starts the download
            _sw.Start();
            client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), _path + "temp.exe");
            InputText.Text = "Downloading Blitz.exe";
            DisableBtn();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;

            // Starts the download
            _sw.Start();
            client.DownloadFileAsync(new Uri("https://aka.ms/vs/16/release/vc_redist.x86.exe"), _path + "temp.exe");
            InputText.Text = "Downloading vc_redist.x86.exe";
            DisableBtn();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Cache successfully cleared");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            Thread.Sleep(1000);
            Uninstall();
            MessageBox.Show("Blitz has been uninstalled", "Blitz Uninstallation", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}