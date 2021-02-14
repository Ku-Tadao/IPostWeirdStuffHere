using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace Blitz_Troubleshooter.Languages.Turkish
{
    /// <summary>
    ///     Interaction logic for Turkish.xaml
    /// </summary>
    public partial class Turkish
    {
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string Localappdata =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

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

        private string _path1;
        private string _path2;

        public Turkish()
        {
            InitializeComponent();
        }

        private void KillBlitz()
        {
            foreach (var proc in Process.GetProcessesByName("Blitz")) proc.Kill();
        }

        private void AppdataBlitz()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz";
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        private void Uninstall()
        {
            foreach (var path in _paths)
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

            MessageBox.Show("Blitz has been uninstalled", "Blitz Uninstallation", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytesIn = double.Parse(e.BytesReceived.ToString());
            var totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            var percentage = bytesIn / totalBytes * 100;

            ProgressBar1.Value = int.Parse(Math.Truncate(percentage).ToString(CultureInfo.InvariantCulture));
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download Completed");
            InputText.Text = "Waiting for input";
            BtnStartDownload.IsEnabled = true;
            Btn3.IsEnabled = true;
            Btn4.IsEnabled = true;
            Btn1.IsEnabled = true;
            Process.Start(_path + "temp.exe");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            Uninstall();
            var client = new WebClient();
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;

            // Starts the download
            client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), _path + "temp.exe");
            InputText.Text = "Downloading Blitz.exe";
            BtnStartDownload.IsEnabled = false;
            Btn1.IsEnabled = false;
            Btn3.IsEnabled = false;
            Btn4.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var process1 = Process.GetProcessesByName("LeagueClient").First();
            if (process1.MainModule != null) _path1 = process1.MainModule.FileName;
            else MessageBox.Show("League is not running, ensure your League client is started");
            var process2 = Process.GetProcessesByName("Blitz").First();
            if (process2.MainModule != null) _path2 = process2.MainModule.FileName;
            else MessageBox.Show("Blitz is not running, ensure your Blitz is started");
            var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
            if (key == null)
            {
                MessageBox.Show("I can't do this, can you ask Ku Tadao#8642 for help D:");
            }
            else
            {
                key.SetValue(_path1, "RUNASADMIN");
                key.SetValue(_path2, "RUNASADMIN");
            }
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
        }
    }
}