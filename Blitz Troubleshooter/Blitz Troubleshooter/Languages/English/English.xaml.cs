using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace Blitz_Troubleshooter.Languages.English
{
    /// <summary>
    ///     Interaction logic for English.xaml
    /// </summary>
    public partial class English
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

        private readonly Stopwatch _sw = new Stopwatch();

        public English()
        {
            InitializeComponent();
        }

        private static void KillBlitz()
        {
            foreach (var proc in Process.GetProcessesByName("Blitz")) proc.Kill();
        }

        private static void KillLeague()
        {
            foreach (var proc in Process.GetProcessesByName("LeagueClient")) proc.Kill();
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
            Labelspeed.Text = $"{e.BytesReceived / 1024d / _sw.Elapsed.TotalSeconds:0.00} kb/s";
            ProgressBar1.Value = int.Parse(Math.Truncate(percentage).ToString(CultureInfo.InvariantCulture));
        }

        private void EnableBtn()
        {
            Btn2.IsEnabled = true;
            Btn3.IsEnabled = true;
            Btn4.IsEnabled = true;
            Btn1.IsEnabled = true;
            Btn5.IsEnabled = true;
            Btn6.IsEnabled = true;
            Btn7.IsEnabled = true;
        }

        private void DisableBtn()
        {
            Btn2.IsEnabled = false;
            Btn1.IsEnabled = false;
            Btn3.IsEnabled = false;
            Btn4.IsEnabled = false;
            Btn5.IsEnabled = false;
            Btn6.IsEnabled = false;
            Btn7.IsEnabled = false;
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

        private void Btn_CommonIssues(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"An error occured, send a screenshot of the following error message to Discord user: \nKu Tadao#8642\n\n{exception}");
            }
        }

        private void Btn_FixOverlay(object sender, RoutedEventArgs e)
        {
            var isleagueclientopen = 0;
            var isblitzclientopen = 0;
            var taskBarProcesses = Process.GetProcesses();
            foreach (var proc in taskBarProcesses)
                try
                {
                    if (proc.ProcessName.ToLower().Contains("leagueclient"))
                    {
                        var filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isleagueclientopen++;
                            if (filepath != null && !filepath.ToLower().Contains("garena")) SetRunAsAdmin(filepath);
                        }
                    }
                    else if (proc.ProcessName.ToLower().Contains("blitz") &&
                             !proc.ProcessName.ToLower().Contains("troubleshooter"))
                    {
                        var filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isblitzclientopen++;
                            SetRunAsAdmin(filepath);
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        $"An error occured, send a screenshot of the following error message to Discord user: \nKu Tadao#8642\n\n{exception}");
                }

            if (isleagueclientopen == 0) MessageBox.Show("Ensure that League of Legends is open");

            if (isblitzclientopen == 0) MessageBox.Show("Ensure that Blitz is open");

            if (isblitzclientopen > 0 && isleagueclientopen > 0) MessageBox.Show("Fixed!");
            KillBlitz();
            KillLeague();
        }

        private void Btn_UnFixOverlay(object sender, RoutedEventArgs e)
        {
            var isleagueclientopen = 0;
            var isblitzclientopen = 0;
            var taskBarProcesses = Process.GetProcesses();
            foreach (var proc in taskBarProcesses)
                if (proc.ProcessName.ToLower().Contains("leagueclient"))
                    isleagueclientopen++;
                else if (proc.ProcessName.ToLower().Contains("blitz") &&
                         !proc.ProcessName.ToLower().Contains("troubleshooter"))
                    isblitzclientopen++;

            if (isblitzclientopen > 0 && isleagueclientopen > 0)
            {
                var counter = 0;
                var shh = 0;
                foreach (var proc in taskBarProcesses)
                    try
                    {
                        if (proc.ProcessName.ToLower().Contains("leagueclient"))
                        {
                            var filepath = proc.MainModule?.FileName;
                            if (proc.MainModule != null)
                            {
                                if (filepath != null && !filepath.ToLower().Contains("garena"))
                                    RemoveRunAsAdmin(filepath);
                                if (shh == 0)
                                {
                                    MessageBox.Show("Removed!");
                                    shh++;
                                }
                            }
                        }
                        else if (proc.ProcessName.ToLower().Contains("blitz") &&
                                 !proc.ProcessName.ToLower().Contains("troubleshooter"))
                        {
                            var filepath = proc.MainModule?.FileName;
                            if (proc.MainModule != null)
                            {
                                RemoveRunAsAdmin(filepath);
                                if (shh == 0)
                                {
                                    MessageBox.Show("Removed!");
                                    shh++;
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        counter++;
                        MessageBox.Show(
                            $"An error occured, send a screenshot of the following error message to Discord user: \nKu Tadao#8642\n\n{exception}");
                    }

                if (counter == 0)
                {
                    KillBlitz();
                    KillLeague();
                }
            }


            if (isleagueclientopen == 0) MessageBox.Show("Ensure that League of Legends is open");

            if (isblitzclientopen == 0) MessageBox.Show("Ensure that Blitz is open");
        }

        private void Btn_FixBoot(object sender, RoutedEventArgs e)
        {
            var isblitzclientopen = 0;
            var taskBarProcesses = Process.GetProcesses();
            foreach (var proc in taskBarProcesses)
                try
                {
                    if (proc.ProcessName.ToLower().Contains("blitz") &&
                        !proc.ProcessName.ToLower().Contains("troubleshooter"))
                    {
                        var filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isblitzclientopen++;
                            SetStartupOnBoot(filepath);
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        $"An error occured, send a screenshot of the following error message to Discord user: \nKu Tadao#8642\n\n{exception}");
                }


            if (isblitzclientopen == 0) MessageBox.Show("Ensure that Blitz is open");

            if (isblitzclientopen > 0) MessageBox.Show("Fixed!");
        }

        private void Btn_UnFixBoot(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveStartup();
                MessageBox.Show("Removed!");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void RemoveStartup()
        {
            var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null)
                throw new InvalidOperationException(
                    @"Cannot open registry key HKCU\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run.");
            using (key)
            {
                key.DeleteValue("com.blitz.app");
            }
        }

        private static void SetStartupOnBoot(string exeFilePath)
        {
            var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null)
                throw new InvalidOperationException(
                    @"Cannot open registry key HKCU\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run.");
            using (key)
            {
                key.SetValue("com.blitz.app", exeFilePath + " --autostart");
            }
        }

        private static void RemoveRunAsAdmin(string exeFilePath)
        {
            try
            {
                const string keyName = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
                using (var key = Registry.LocalMachine.OpenSubKey(keyName, true))
                {
                    key?.DeleteValue(exeFilePath);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void SetRunAsAdmin(string exeFilePath)
        {
            var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
            if (key == null)
                throw new InvalidOperationException(
                    @"Cannot open registry key HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers.");
            using (key)
            {
                key.SetValue(exeFilePath, "RUNASADMIN");
            }
        }

        private void Btn_ClearCache(object sender, RoutedEventArgs e)
        {
            try
            {
                KillBlitz();
                Thread.Sleep(1000);
                AppdataBlitz();
                MessageBox.Show("Cache successfully cleared");
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"An error occured, send a screenshot of the following error message to Discord user: \nKu Tadao#8642\n\n{exception}");
            }
        }

        private void Btn_UninstallBlitz(object sender, RoutedEventArgs e)
        {
            try
            {
                KillBlitz();
                Thread.Sleep(1000);
                Uninstall();
                MessageBox.Show("Blitz has been uninstalled", "Blitz Uninstallation", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"An error occured, send a screenshot of the following error message to Discord user: \nKu Tadao#8642\n\n{exception}");
            }
        }
    }
}