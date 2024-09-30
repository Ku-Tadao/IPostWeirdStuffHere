using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Blitz_Troubleshooter
{
    public partial class Troubleshooter : Window
    {
        #region Fields and Constants

        private readonly SolidColorBrush DarkColor = new SolidColorBrush(Color.FromRgb(14, 16, 21));
        private readonly SolidColorBrush DarkBGColor = new SolidColorBrush(Color.FromRgb(39, 42, 48));
        private readonly SolidColorBrush BlueColor = new SolidColorBrush(Color.FromRgb(18, 26, 43));
        private readonly SolidColorBrush BlueBGColor = new SolidColorBrush(Color.FromRgb(38, 52, 80));

        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string tempPath = Path.GetTempPath();

        private readonly string[] blitzPaths =
        {
            Path.Combine(LocalAppDataPath, "blitz-updater"),
            Path.Combine(LocalAppDataPath, "Programs", "blitz-core"),
            Path.Combine(LocalAppDataPath, "Programs", "blitz-delta-updater"),
            Path.Combine(LocalAppDataPath, "Programs", "Blitz"),
            Path.Combine(LocalAppDataPath, "Blitz"),
            Path.Combine(AppDataPath, "Blitz"),
            Path.Combine(AppDataPath, "blitz-core"),
            Path.Combine(AppDataPath, "Blitz-helpers")
        };

        private readonly ResourceDictionary languageDictionary = new ResourceDictionary();
        private const string BlitzInstallPath = @"%localappdata%\programs\blitz";

        #endregion

        #region Initialization and UI Configuration

        public Troubleshooter()
        {
            InitializeComponent();
            Grid.Background = DarkColor;
            Window1.Title = "Blitz Troubleshooter";
            SetLanguageDictionary(languageDictionary);
            Loaded += Troubleshooter_Loaded;
        }

        private void Troubleshooter_Loaded(object sender, RoutedEventArgs e)
        {
            MaxWidth = 800;
            SizeToContent = SizeToContent.Width;
        }

        #endregion

        #region Language Management

        private void SetLanguageDictionary(ResourceDictionary resourceDictionary)
        {
            GetLanguageResource(resourceDictionary);
            Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void GetLanguageResource(ResourceDictionary resourceDictionary, string lang = "")
        {
            var culture = string.IsNullOrEmpty(lang) ? Thread.CurrentThread.CurrentCulture.ToString() : lang;
            string resourcePath = culture switch
            {
                "en-US" => "..\\Resources\\StringResource.xaml",
                "pl-PL" => "..\\Resources\\StringResource.pl-PL.xaml",
                "de-DE" => "..\\Resources\\StringResource.de-DE.xaml",
                "pt-PT" => "..\\Resources\\StringResource.pt-PT.xaml",
                "tr-TR" => "..\\Resources\\StringResource.tr-TR.xaml",
                "fr-FR" => "..\\Resources\\StringResource.fr-FR.xaml",
                "ru-RU" => "..\\Resources\\StringResource.ru-RU.xaml",
                _ => "..\\Resources\\StringResource.xaml",
            };
            resourceDictionary.Source = new Uri(resourcePath, UriKind.Relative);
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CbLang.SelectedValue != null)
            {
                Resources.MergedDictionaries.Remove(languageDictionary);
                GetLanguageResource(languageDictionary, CbLang.SelectedValue.ToString());
                Resources.MergedDictionaries.Add(languageDictionary);
            }
            SizeToContent = SizeToContent.Width;
        }

        #endregion

        #region Blitz Management

        private void KillBlitzProcesses()
        {
            var blitzProcesses = Process.GetProcessesByName("Blitz");
            foreach (var process in blitzProcesses)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{languageDictionary["error"]}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteAppDataBlitzFolder()
        {
            var path = Path.Combine(AppDataPath, "Blitz");
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{languageDictionary["error"]}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UninstallBlitz()
        {
            foreach (var path in blitzPaths)
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        Directory.Delete(path, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{languageDictionary["error"]}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
                            if (subKey?.GetValue("DisplayName") is string displayName && displayName.Contains(blitzDisplayName))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Download and Installation

        private async void BtnFixCommonIssues_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisableButtons();
                KillBlitzProcesses();
                await Task.Delay(2000);
                UninstallBlitz();

                InputText.Text = (string)languageDictionary["dloading"];
                var tempFilePath = Path.Combine(tempPath, "temp.exe");

                using (var client = new HttpClient())
                using (var response = await client.GetAsync("https://blitz.gg/download/win", HttpCompletionOption.ResponseHeadersRead))
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                    var buffer = new byte[8192];
                    long bytesRead;

                    using (var downloadStream = await response.Content.ReadAsStreamAsync())
                    {
                        while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, (int)bytesRead);

                            var progressPercentage = (double)fileStream.Length / totalBytes * 100;
                            ProgressBar1.Value = progressPercentage;
                        }
                    }
                }

                InputText.Text = (string)languageDictionary["dloadcompleted"];
                Process.Start(tempFilePath);
                EnableButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{languageDictionary["error"]}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EnableButtons();
            }
        }

        private async void BtnManualInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var localBlitzFolderPath = Path.Combine(LocalAppDataPath, "Programs", "Blitz");

                Directory.CreateDirectory(localBlitzFolderPath);

                var blitzZipPath = Path.Combine(localBlitzFolderPath, "blitzportable.zip");
                var blitzExePath = Path.Combine(localBlitzFolderPath, "blitz.exe");

                InputText.Text = (string)languageDictionary["dloading"];

                var progressHandler = new ProgressMessageHandler();
                var client = HttpClientFactory.Create(progressHandler);

                progressHandler.HttpReceiveProgress += (s, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar1.Value = args.ProgressPercentage;
                    });
                };

                using (var response = await client.GetAsync("https://github.com/Ku-Tadao/IPostWeirdStuffHere/raw/master/Blitz.gg/Portable/blitzportable.zip", HttpCompletionOption.ResponseHeadersRead))
                using (var fileStream = new FileStream(blitzZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                ZipFile.ExtractToDirectory(blitzZipPath, localBlitzFolderPath);

                InputText.Text = (string)languageDictionary["dloadcompleted"];
                Process.Start(blitzExePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{languageDictionary["error"]}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Cache Management

        private async void BtnClearCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                KillBlitzProcesses();
                await Task.Delay(1000);
                DeleteAppDataBlitzFolder();
                MessageBox.Show((string)languageDictionary["cachemsg"], "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"{languageDictionary["error"]}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Uninstall Blitz

        private async void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsBlitzInstalled())
                {
                    KillBlitzProcesses();
                    await Task.Delay(1000);
                    UninstallBlitz();
                    MessageBox.Show((string)languageDictionary["removedmsg"], "Blitz Un-installation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show((string)languageDictionary["notinstalled"], "Blitz Un-installation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{languageDictionary["error"]}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region UI Theme Handling

        private void BtnColor_Click(object sender, RoutedEventArgs e)
        {
            if (Grid.Background != BlueColor)
            {
                BtnColor.Content = "Blue";
                BtnColor.Background = BlueBGColor;
                Grid.Background = BlueColor;

                Btn1.Background = BlueBGColor;
                Btn6.Background = BlueBGColor;
                Btn7.Background = BlueBGColor;
                Btn8.Background = BlueBGColor;
            }
            else
            {
                BtnColor.Content = "Dark";
                BtnColor.Background = DarkBGColor;
                Grid.Background = DarkColor;

                Btn1.Background = DarkBGColor;
                Btn6.Background = DarkBGColor;
                Btn7.Background = DarkBGColor;
                Btn8.Background = DarkBGColor;
            }
        }

        #endregion

        #region Button Enable/Disable

        private void EnableButtons()
        {
            Btn1.IsEnabled = true;
            Btn6.IsEnabled = true;
            Btn7.IsEnabled = true;
            Btn8.IsEnabled = true;
        }

        private void DisableButtons()
        {
            Btn1.IsEnabled = false;
            Btn6.IsEnabled = false;
            Btn7.IsEnabled = false;
            Btn8.IsEnabled = false;
        }

        #endregion
    }
}
