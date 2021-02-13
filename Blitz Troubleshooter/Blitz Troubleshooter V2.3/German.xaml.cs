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
    ///     Interaction logic for German.xaml
    /// </summary>
    public partial class German
    {
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string Localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private readonly string[] _paths =
        {
            Localappdata + "\\blitz-updater",
            Localappdata + "\\Programs\\blitz-core",
            Appdata + "\\Blitz",
            Appdata + "\\blitz-core",
            Appdata + "\\Blitz-helpers",
            Localappdata + "\\Programs\\Blitz"
        };

        private readonly string _path = Path.GetTempPath();

        public German()
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

            MessageBox.Show("Blitz wurde deinstalliert ", " Blitz-Deinstallation ", MessageBoxButton.OK,
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
            MessageBox.Show("Download abgeschlossen");
            InputText.Text = "Warten auf Eingabe";
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
            InputText.Text = "Blitz.exe wird heruntergeladen";
            BtnStartDownload.IsEnabled = false;
            Btn1.IsEnabled = false;
            Btn3.IsEnabled = false;
            Btn4.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;

            // Starts the download
            client.DownloadFileAsync(new Uri("https://aka.ms/vs/16/release/vc_redist.x86.exe"), _path + "temp.exe");
            InputText.Text = "vc_redist.x86.exe wird heruntergeladen";
            BtnStartDownload.IsEnabled = false;
            Btn1.IsEnabled = false;
            Btn3.IsEnabled = false;
            Btn4.IsEnabled = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            Thread.Sleep(1000);
            AppdataBlitz();
            MessageBox.Show("Cache erfolgreich geleert");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            KillBlitz();
            Thread.Sleep(1000);
            Uninstall();
        }
    }
}