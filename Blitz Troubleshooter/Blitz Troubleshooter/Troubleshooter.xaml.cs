using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace Blitz_Troubleshooter
{
    /// <summary>
    /// Interaction logic for Troubleshooter.xaml
    /// </summary>
    public partial class Troubleshooter
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

        ResourceDictionary dict = new ResourceDictionary();

        public void SetLanguageDictionary(ResourceDictionary resourceDictionary)
        {
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "en-US":
                    resourceDictionary.Source = new Uri("..\\Resources\\StringResource.xaml",
                        UriKind.Relative);
                    break;
                case "pl-PL":
                    resourceDictionary.Source = new Uri("..\\Resources\\StringResource.pl-PL.xaml",
                        UriKind.Relative);
                    break;
                case "de-DE":
                    resourceDictionary.Source = new Uri("..\\Resources\\StringResource.de-DE.xaml",
                        UriKind.Relative);
                    break;
                case "pt-PT":
                    resourceDictionary.Source = new Uri("..\\Resources\\StringResource.pt-PT.xaml",
                        UriKind.Relative);
                    break;
                case "tr-TR":
                    resourceDictionary.Source = new Uri("..\\Resources\\StringResource.tr-TR.xaml",
                        UriKind.Relative);
                    break;
                case "fr-FR":
                    resourceDictionary.Source = new Uri("..\\Resources\\StringResource.fr-FR.xaml",
                        UriKind.Relative);
                    break;
                default:
                    resourceDictionary.Source = new Uri("..\\Resources\\StringResource.xaml",
                        UriKind.Relative);
                    break;
            }
            Resources.MergedDictionaries.Add(resourceDictionary);
        }

        public Troubleshooter()
        {
            InitializeComponent();
            Window1.Title = "Version 2.10";
            SetLanguageDictionary(dict);
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar1.Width = DpSize.ActualWidth;
            Application.Current.MainWindow.Width = ProgressBar1.Width + 45;
            Grid.Width = ProgressBar1.Width + 45;
        }

        private static void KillBlitz()
        {
            foreach (Process proc in Process.GetProcessesByName("Blitz")) proc.Kill();
        }

        private static void KillLeague()
        {
            foreach (Process proc in Process.GetProcessesByName("LeagueClient")) proc.Kill();
        }

        private static void AppdataBlitz()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blitz";
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        private void Uninstall()
        {
            foreach (string path in _paths)
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
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
            MessageBox.Show((string)dict["dload"], (string)dict["dloadcompleted"], MessageBoxButton.OK, MessageBoxImage.Information);
            InputText.Text = (string)dict["dloadcompleted"];
            Labelspeed.Text = null;
            EnableBtn();
            Process.Start(_path + "temp.exe");
            _sw.Reset();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show((string)dict["warningclose"], "Warning!", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        KillLeague();
                        Thread.Sleep(1000);
                        KillBlitz();
                        Thread.Sleep(2000);
                        Uninstall();
                        WebClient client = new WebClient();
                        client.DownloadProgressChanged += client_DownloadProgressChanged;
                        client.DownloadFileCompleted += client_DownloadFileCompleted;

                        // Starts the download
                        _sw.Start();
                        client.DownloadFileAsync(new Uri("https://blitz.gg/download/win"), _path + "temp.exe");
                        InputText.Text = (string)dict["dloading"];
                        DisableBtn();

                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nKu Tadao#8642\n\n{exception}");
            }
        }

        private void BtnFixOverlayClick(object sender, RoutedEventArgs e)
        {
            int isleagueclientopen = 0;
            int isblitzclientopen = 0;
            Process[] taskBarProcesses = Process.GetProcesses();
            foreach (Process proc in taskBarProcesses)
                try
                {
                    if (proc.ProcessName.ToLower().Contains("leagueclient"))
                    {
                        string filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isleagueclientopen++;
                            if (filepath != null && !filepath.ToLower().Contains("garena")) SetRunAsAdmin(filepath);
                        }
                    }
                    else if (proc.ProcessName.ToLower().Contains("blitz") &&
                             !proc.ProcessName.ToLower().Contains("troubleshooter"))
                    {
                        string filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isblitzclientopen++;
                            SetRunAsAdmin(filepath);
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"{dict["error"]}\nKu Tadao#8642\n\n{exception}");
                }

            if (isleagueclientopen == 0) MessageBox.Show((string)dict["leaguemsg"]);

            if (isblitzclientopen == 0) MessageBox.Show((string)dict["blitzmsg"]);

            if (isblitzclientopen > 0 && isleagueclientopen > 0) MessageBox.Show((string)dict["fixedmsg"]);
        }

        private void BtnRemoveOverlayFixClick(object sender, RoutedEventArgs e)
        {
            int isleagueclientopen = 0;
            int isblitzclientopen = 0;
            Process[] taskBarProcesses = Process.GetProcesses();
            foreach (Process proc in taskBarProcesses)
                try
                {
                    if (proc.ProcessName.ToLower().Contains("leagueclient"))
                    {
                        string filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isleagueclientopen++;
                            if (filepath != null && !filepath.ToLower().Contains("garena")) RemoveRunAsAdmin(filepath);
                        }
                    }
                    else if (proc.ProcessName.ToLower().Contains("blitz") &&
                             !proc.ProcessName.ToLower().Contains("troubleshooter"))
                    {
                        string filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isblitzclientopen++;
                            RemoveRunAsAdmin(filepath);
                            MessageBox.Show((string)dict["removedmsg"]);

                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"{dict["error"]}\nKu Tadao#8642\n\n{exception}");
                }

            if (isleagueclientopen == 0) MessageBox.Show((string)dict["leaguemsg"]);

            if (isblitzclientopen == 0) MessageBox.Show((string)dict["blitzmsg"]);
        }

        private void BtnFixBootClick(object sender, RoutedEventArgs e)
        {
            int isblitzclientopen = 0;
            Process[] taskBarProcesses = Process.GetProcesses();
            foreach (Process proc in taskBarProcesses)
                try
                {
                    if (proc.ProcessName.ToLower().Contains("blitz") &&
                        !proc.ProcessName.ToLower().Contains("troubleshooter"))
                    {
                        string filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isblitzclientopen++;
                            SetStartupOnBoot(filepath);
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"{dict["error"]}\nKu Tadao#8642\n\n{exception}");
                }


            if (isblitzclientopen == 0) MessageBox.Show((string)dict["blitzmsg"]);

            if (isblitzclientopen > 0) MessageBox.Show((string)dict["fixedmsg"]);
        }

        private void BtnRemoveBootFixClick(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveStartup();
                MessageBox.Show((string)dict["removedmsg"]);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void RemoveStartup()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(
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
            RegistryKey key = Registry.LocalMachine.OpenSubKey(
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
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
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
            RegistryKey key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
            if (key == null)
                throw new InvalidOperationException(
                    @"Cannot open registry key HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers.");
            using (key)
            {
                key.SetValue(exeFilePath, "RUNASADMIN");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                KillBlitz();
                Thread.Sleep(1000);
                AppdataBlitz();
                MessageBox.Show((string)dict["cachemsg"]);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nKu Tadao#8642\n\n{exception}");
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new IndexOutOfRangeException();
                //KillBlitz();
                //Thread.Sleep(1000);
                //Uninstall();
                //MessageBox.Show("Blitz has been uninstalled", "Blitz Un-installation", MessageBoxButton.OK,
                //    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nKu Tadao#8642\n\n{exception}");
            }
        }

    }
}
