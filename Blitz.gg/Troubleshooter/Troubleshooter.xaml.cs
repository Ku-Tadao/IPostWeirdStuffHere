using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Net.Http;
using System.Text.Json;
using System.IO.Compression;
using System.Net.Http.Handlers;

namespace Blitz_Troubleshooter
{
    public partial class Troubleshooter
    {
        private readonly SolidColorBrush DarkColor = new SolidColorBrush(Color.FromRgb(14, 16, 21));
        private readonly SolidColorBrush DarkBGColor = new SolidColorBrush(Color.FromRgb(39, 42, 48));
        private readonly SolidColorBrush BlueColor = new SolidColorBrush(Color.FromRgb(18, 26, 43));
        private readonly SolidColorBrush BlueBGColor = new SolidColorBrush(Color.FromRgb(38, 52, 80));
        private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string Localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string _path = Path.GetTempPath();
        private readonly string[] _paths =
        {
            Path.Combine(Localappdata, "blitz-updater"),
            Path.Combine(Localappdata, "Programs", "blitz-core"),
            Path.Combine(Localappdata, "Programs", "blitz-delta-updater"),
            Path.Combine(Localappdata, "Programs", "Blitz"),
            Path.Combine(Localappdata, "Blitz"),
            Path.Combine(Appdata, "Blitz"),
            Path.Combine(Appdata, "blitz-core"),
            Path.Combine(Appdata, "Blitz-helpers")
        };
        private readonly Stopwatch _sw = new Stopwatch();
        private readonly ResourceDictionary dict = new ResourceDictionary();

        public Uri GetLang(ResourceDictionary resourceDictionary, string lang = "")
        {
            var text = string.IsNullOrEmpty(lang) ? Thread.CurrentThread.CurrentCulture.ToString() : lang;
            switch (text)
            {
                case "en-US":
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.xaml");
                case "pl-PL":
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.pl-PL.xaml");
                case "de-DE":
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.de-DE.xaml");
                case "pt-PT":
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.pt-PT.xaml");
                case "tr-TR":
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.tr-TR.xaml");
                case "fr-FR":
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.fr-FR.xaml");
                case "ru-RU":
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.ru-RU.xaml");
                default:
                    return SetResourceSource(resourceDictionary, "..\\Resources\\StringResource.xaml");
            }
        }

        private Uri SetResourceSource(ResourceDictionary resourceDictionary, string path)
        {
            resourceDictionary.Source = new Uri(path, UriKind.Relative);
            return resourceDictionary.Source;
        }

        public void SetLanguageDictionary(ResourceDictionary resourceDictionary)
        {
            GetLang(dict);
            Resources.MergedDictionaries.Add(resourceDictionary);
        }

        public Troubleshooter()
        {
            InitializeComponent();
            Grid.Background = DarkColor;
            Window1.Title = "Version 2.25";
            SetLanguageDictionary(dict);
            this.Loaded += Troubleshooter_Loaded;

        }

        private void Troubleshooter_Loaded(object sender, RoutedEventArgs e)
        {
            Btn2.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Btn4.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            double maxWidth = Math.Max(Btn2.DesiredSize.Width, Btn4.DesiredSize.Width);
            double minWidth = Math.Max(Btn3.DesiredSize.Width, Btn5.DesiredSize.Width);

            spmain.Width = maxWidth + minWidth + 50; // Adjust the constant based on your layout requirements
            this.SizeToContent = SizeToContent.Width;
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
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
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
            Btn8.IsEnabled = true;
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
            Btn8.IsEnabled = false;

        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show((string)dict["dload"], (string)dict["dloadcompleted"], MessageBoxButton.OK, MessageBoxImage.Information);
            InputText.Text = (string)dict["dloadcompleted"];
            Labelspeed.Text = null;
            EnableBtn();
            Process.Start(_path + "temp.exe");
            _sw.Reset();
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                KillBlitz();
                await Task.Delay(2000);
                Uninstall();

                var client = new HttpClient();

                using (var response = await client.GetAsync("https://blitz.gg/download/win", HttpCompletionOption.ResponseHeadersRead))
                using (var fileStream = new FileStream(_path + "temp.exe", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Get the total size of the file from the Content-Length header
                    var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();

                    var buffer = new byte[8192];
                    var bytesRead = 0L;

                    using (var downloadStream = await response.Content.ReadAsStreamAsync())
                    {
                        while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, (int)bytesRead);

                            // Calculate the progress as a percentage and update the ProgressBar
                            var progressPercentage = (double)fileStream.Length / totalBytes * 100;
                            ProgressBar1.Value = progressPercentage;
                        }
                    }
                }

                InputText.Text = (string)dict["dloading"];
                DisableBtn();

                // Open the downloaded file
                Process.Start(_path + "temp.exe");
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}");
            }
        }



        private void BtnFixOverlayClick(object sender, RoutedEventArgs e)
        {
            var gameOpenCounts = CheckGamesOpen();

            if (gameOpenCounts.Values.Any(count => count > 0))
            {
                var warningMessage = dict["gamesareopen"] + "\n";

                foreach (var game in gameOpenCounts)
                {
                    if (game.Value > 0)
                    {
                        warningMessage += $"{game.Key}: {game.Value} instance(s)\n";
                    }
                }

                warningMessage += dict["closegames"];
                MessageBox.Show(warningMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var isleagueclientopen = 0;
            var isblitzclientopen = 0;
            var taskBarProcesses = Process.GetProcesses();
            foreach (var proc in taskBarProcesses)
            {
                try
                {
                    if (proc.ProcessName.ToLower().Contains("leagueclient"))
                    {
                        var filepath = proc.MainModule?.FileName;
                        if (proc.MainModule != null)
                        {
                            isleagueclientopen++;
                            if (filepath != null) SetRunAsAdmin(filepath);
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
                    MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}");
                }
            }

            if (isleagueclientopen == 0) MessageBox.Show((string)dict["leaguemsg"]);

            if (isblitzclientopen == 0) MessageBox.Show((string)dict["blitzmsg"]);

            if (isblitzclientopen > 0 && isleagueclientopen > 0) MessageBox.Show((string)dict["fixedmsg"]);
        }

        private Dictionary<string, int> CheckGamesOpen()
        {

            var gameOpenCounts = new Dictionary<string, int>
                {
                    {"League of Legends", 0},
                    {"CS:GO", 0},
                    {"Valorant", 0},
                    {"Fortnite", 0},
                    {"Escape from Tarkov", 0},
                    {"Apex Legends", 0},
                    {"Minecraft", 0},
                };

            var gameProcessNames = new Dictionary<string, string>
                {
                    {"League of Legends", "leagueclient"},
                    {"CS:GO", "csgo"},
                    {"Valorant", "valorant"},
                    {"Fortnite", "fortnite"},
                    {"Escape from Tarkov", "eft"},
                    {"Apex Legends", "r5apex"},
                    {"Minecraft", "minecraft"},
                };

            var allProcesses = Process.GetProcesses();

            foreach (var process in allProcesses)
            {
                var processName = process.ProcessName.ToLower();
                foreach (var game in gameProcessNames)
                {
                    if (processName.Contains(game.Value))
                    {
                        gameOpenCounts[game.Key]++;
                    }
                }
            }
            return gameOpenCounts;
        }

        private void BtnRemoveAdminClick(object sender, RoutedEventArgs e)
        {
            var taskBarProcesses = Process.GetProcesses();

            var leagueClients = taskBarProcesses.Where(proc => proc.ProcessName.ToLower().Contains("leagueclient")).Take(1);
            var blitzClients = taskBarProcesses.Where(proc => proc.ProcessName.ToLower().Contains("blitz") && !proc.ProcessName.ToLower().Contains("troubleshooter")).Take(1);

            var isLeagueClientOpen = false;
            var isBlitzClientOpen = false;

            foreach (var leagueClient in leagueClients)
            {
                var filepath = leagueClient.MainModule?.FileName;
                if (filepath != null)
                {
                    isLeagueClientOpen = true;
                    RemoveRunAsAdmin(filepath);
                }
            }

            foreach (var blitzClient in blitzClients)
            {
                var filepath = blitzClient.MainModule?.FileName;
                if (filepath != null)
                {
                    isBlitzClientOpen = true;
                    RemoveRunAsAdmin(filepath);
                }
            }

            if (!isLeagueClientOpen) MessageBox.Show((string)dict["leaguemsg"]);
            if (!isBlitzClientOpen) MessageBox.Show((string)dict["blitzmsg"]);
        }

        private void BtnFixBootClick(object sender, RoutedEventArgs e)
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
                    MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}");
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
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}", "Already removed / Non existent?");
            }
        }

        private void RemoveStartup()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null && key.GetValue("com.blitz.app") != null)
                    {
                        key.DeleteValue("com.blitz.app");
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}", "Already removed / Non existent?");
            }
        }

        private static void SetStartupOnBoot(string exeFilePath)
        {
            var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", true) ?? throw new InvalidOperationException(
                    @"Cannot open registry key HKCU\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run.");
            using (key)
            {
                key.SetValue("com.blitz.app", Path.Combine(exeFilePath, "--autostart"));
            }
        }


        private void RemoveRunAsAdmin(string exeFilePath)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true))
                {
                    if (key != null && key.GetValue(exeFilePath) != null)
                    {
                        key.DeleteValue(exeFilePath);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}", "Already removed / Non existent?");
            }
        }

        private void SetRunAsAdmin(string exeFilePath)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true))
                {
                    if (key != null)
                    {
                        key.SetValue(exeFilePath, "RUNASADMIN");
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}");
            }
        }

        private async void Btn8_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var blitzFolderPath = Path.Combine(localAppDataPath, "programs", "blitz");

                // Ensure the Blitz folder exists
                Directory.CreateDirectory(blitzFolderPath);

                var progressHandler = new ProgressMessageHandler();
                var client = HttpClientFactory.Create(progressHandler);

                // Update the progress bar as the file is downloaded
                progressHandler.HttpReceiveProgress += (s, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar1.Value = args.ProgressPercentage;
                    });
                };

                using (var response = await client.GetAsync("https://github.com/Ku-Tadao/IPostWeirdStuffHere/raw/master/Blitz.gg/Portable/blitzportable.zip", HttpCompletionOption.ResponseHeadersRead))
                using (var fileStream = new FileStream(Path.Combine(blitzFolderPath, "blitzportable.zip"), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Download the file
                    await response.Content.CopyToAsync(fileStream);
                }

                ZipFile.ExtractToDirectory(Path.Combine(blitzFolderPath, "blitzportable.zip"), blitzFolderPath);
                string blitzExePath = Path.Combine(blitzFolderPath, "blitz.exe");
                Process.Start(blitzExePath);

            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}");
            }
        }


        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                KillBlitz();
                await Task.Delay(1000);
                AppdataBlitz();
                MessageBox.Show((string)dict["cachemsg"]);
            }
            catch (IOException exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception.Message}");
            }
        }


        private bool IsBlitzInstalled()
        {
            var blitzRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var blitzDisplayName = "Blitz";

            using (var key = Registry.CurrentUser.OpenSubKey(blitzRegistryPath))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey != null)
                            {
                                if (subKey.GetValue("DisplayName") is string displayName && displayName.Contains(blitzDisplayName))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }


        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsBlitzInstalled())
                {
                    KillBlitz();
                    await Task.Delay(1000);
                    Uninstall();
                    MessageBox.Show((string)dict["removedmsg"], "Blitz Un-installation", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show((string)dict["notinstalled"], "Blitz Un-installation", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{dict["error"]}\nku_tadao\n\n{exception}");
            }
        }


        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Resources.MergedDictionaries.Remove(dict);
            GetLang(dict, CbLang.SelectedValue.ToString());
            Resources.MergedDictionaries.Add(dict);

            Btn2.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Btn4.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            double maxWidth = Math.Max(Btn2.DesiredSize.Width, Btn4.DesiredSize.Width);
            double minWidth = Math.Max(Btn3.DesiredSize.Width, Btn5.DesiredSize.Width);

            spmain.Width = maxWidth + minWidth + 50; // Adjust the constant based on your layout requirements
            this.SizeToContent = SizeToContent.Width;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Grid.Background != BlueColor)
            {
                BtnColor.Content = "Blue";
                BtnColor.Background = BlueBGColor;
                Grid.Background = BlueColor;

                Btn1.Background = BlueBGColor;
                Btn2.Background = BlueBGColor;
                Btn3.Background = BlueBGColor;
                Btn4.Background = BlueBGColor;
                Btn5.Background = BlueBGColor;
                Btn6.Background = BlueBGColor;
                Btn7.Background = BlueBGColor;
            }
            else
            {
                BtnColor.Content = "Dark";
                BtnColor.Background = DarkBGColor;
                Grid.Background = DarkColor;

                Btn1.Background = DarkBGColor;
                Btn2.Background = DarkBGColor;
                Btn3.Background = DarkBGColor;
                Btn4.Background = DarkBGColor;
                Btn5.Background = DarkBGColor;
                Btn6.Background = DarkBGColor;
                Btn7.Background = DarkBGColor;
            }
        }


        private static readonly HttpClient client = new HttpClient();

        private async Task<List<string>> GetPortableFileNamesAsync()
        {
            var response = await client.GetAsync("https://api.github.com/repos/Ku-Tadao/IPostWeirdStuffHere/contents/Blitz.gg/Portable");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var contents = JsonSerializer.Deserialize<List<GitHubContent>>(responseContent);
            var fileNames = new List<string>();
            foreach (var content in contents)
            {
                if (content.Type == "file")
                {
                    fileNames.Add(content.Name);
                }
            }
            return fileNames;
        }

        private class GitHubContent
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }

        



        private const string localAppDataPath = @"%localappdata%\programs\blitz";
        private const string portableFileUrl = "https://github.com/Ku-Tadao/IPostWeirdStuffHere/raw/master/Blitz.gg/Portable/blitzportable.zip";

        private async Task DownloadAndExtractPortableFileAsync()
        {
            var localFilePath = Path.Combine(localAppDataPath, "blitzportable.zip");
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://blitz.gg/download/win");
                using (var fileStream = new FileStream(_path + "temp.exe", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
            ZipFile.ExtractToDirectory(localFilePath, localAppDataPath);
        }




    private void EnsureBlitzFolderExists(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!Directory.Exists(localAppDataPath))
            {
                Directory.CreateDirectory(localAppDataPath);
            }
            else
            {
                KillBlitz();

                Directory.Delete(localAppDataPath, true);
                Directory.CreateDirectory(localAppDataPath);

                _ = DownloadAndExtractPortableFileAsync();

                MessageBox.Show("Complete!");
            }
        }




}
}
