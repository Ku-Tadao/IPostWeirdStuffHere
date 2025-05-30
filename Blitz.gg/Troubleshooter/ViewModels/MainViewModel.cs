using BlitzTroubleshooter.Models;
using BlitzTroubleshooter.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlitzTroubleshooter.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IBlitzService _blitzService;
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [ObservableProperty]
        private double progressValue = 0;

        [ObservableProperty]
        private bool isOperationInProgress = false;

        [ObservableProperty]
        private BlitzInstallation? blitzInstallation;

        [ObservableProperty]
        private ObservableCollection<string> runningGames = new();

        [ObservableProperty]
        private bool isDarkTheme = true;

        public string WindowTitle { get; private set; }

        public string NextThemeDisplayName => IsDarkTheme ? GetStringResource("themeNameBlue") : GetStringResource("themeNameDark");

        public MainViewModel(
            IBlitzService blitzService,
            ILogger<MainViewModel> logger)
        {
            _blitzService = blitzService;
            _logger = logger;

            StatusMessage = GetStringResource("statusReady");
            WindowTitle = GetStringResource("windowTitle") + " v3";

            ApplyTheme();
        }

        public async Task LoadAsync()
        {
            await InitializeAsync();
        }

        #region Localization Helpers
        private string GetStringResource(string key)
        {
            try
            {
                return Application.Current.FindResource(key) as string ?? key;
            }
            catch
            {
                _logger.LogWarning("Resource key not found or error loading: {Key}", key);
                return key;
            }
        }

        private string GetFormattedStringResource(string key, params object[] args)
        {
            try
            {
                string? format = Application.Current.FindResource(key) as string;
                return format != null ? string.Format(format, args) : key;
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Format exception for resource key: {Key} with args: {Args}", key, string.Join(",", args));
                return key;
            }
            catch
            {
                _logger.LogWarning("Resource key not found or error loading for formatting: {Key}", key);
                return key;
            }
        }
        #endregion

        private async Task InitializeAsync()
        {
            try
            {
                await RefreshBlitzInstallationAsync();
                await RefreshRunningGamesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRITICAL ERROR during MainViewModel.InitializeAsync");
                StatusMessage = GetStringResource("statusErrorInitializing");
            }
        }

        [RelayCommand]
        private async Task RefreshBlitzInstallationAsync()
        {
            try
            {
                StatusMessage = GetStringResource("statusCheckingBlitzInstallation");
                BlitzInstallation = await _blitzService.GetInstallationInfoAsync();

                StatusMessage = BlitzInstallation.Status switch
                {
                    BlitzInstallationStatus.Installed => GetFormattedStringResource("statusBlitzInstalledAt", BlitzInstallation.InstallPath ?? GetStringResource("pathNotFoundFallback")),
                    BlitzInstallationStatus.Corrupted => GetStringResource("statusBlitzCorruptedMessage"),
                    BlitzInstallationStatus.NotInstalled => GetStringResource("statusBlitzNotDetected"),
                    _ => GetStringResource("statusUnknownInstallation")
                };

                if (BlitzInstallation.IsCorrupted)
                {
                    _logger.LogWarning("Corrupted Blitz installation detected. Missing executables: {MissingExes}",
                        string.Join(", ", BlitzInstallation.MissingExecutables));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing Blitz installation");
                StatusMessage = GetStringResource("statusErrorCheckingBlitzInstallation");
            }
        }


        [RelayCommand]
        private async Task RefreshRunningGamesAsync()
        {
            try
            {
                StatusMessage = GetStringResource("statusCheckingRunningGames");
                var games = await _blitzService.GetRunningGamesAsync();

                RunningGames.Clear();
                foreach (var game in games)
                {
                    RunningGames.Add(game);
                }

                StatusMessage = games.Count > 0
                    ? GetFormattedStringResource("statusRunningGamesList", string.Join(", ", games))
                    : GetStringResource("statusNoSupportedGamesRunning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking running games");
                StatusMessage = GetStringResource("statusErrorCheckingRunningGames");
            }
        }

        [RelayCommand]
        private async Task FixAllIssuesAsync()
        {
            if (IsOperationInProgress) return;
            try
            {
                IsOperationInProgress = true;
                ProgressValue = 0;

                var statusProgress = new Progress<string>(statusKey => StatusMessage = GetStringResource(statusKey));
                var downloadProgress = new Progress<double>(progress => ProgressValue = progress);

                var result = await _blitzService.FixAllIssuesAsync(statusProgress, downloadProgress);

                if (result.IsSuccess)
                {
                    MessageBox.Show(GetStringResource(result.Message), GetStringResource("titleSuccess"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await RefreshBlitzInstallationAsync();
                }
                else if (result.Message.StartsWith("RUNNING_GAMES_DETECTED_KEY:"))
                {
                    var gamesList = result.Message.Substring("RUNNING_GAMES_DETECTED_KEY:".Length);

                    var confirmResult = MessageBox.Show(
                        GetFormattedStringResource("promptRunningGamesDetectedMessage", gamesList),
                        GetStringResource("titleRunningGamesDetected"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        StatusMessage = GetStringResource("statusProceedingDespiteGames");
                        ProgressValue = 0;

                        var forceResult = await _blitzService.FixAllIssuesForceAsync(statusProgress, downloadProgress);

                        if (forceResult.IsSuccess)
                        {
                            MessageBox.Show(GetStringResource(forceResult.Message), GetStringResource("titleSuccess"),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await RefreshBlitzInstallationAsync();
                        }
                        else
                        {
                            MessageBox.Show(GetFormattedStringResource("messageErrorPrefix", GetStringResource(forceResult.Message)), GetStringResource("titleError"),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        StatusMessage = GetStringResource("statusTroubleshootingCancelledCloseGames");
                    }
                }
                else
                {
                    MessageBox.Show(GetFormattedStringResource("messageErrorPrefix", GetStringResource(result.Message)), GetStringResource("titleError"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing all issues");
                MessageBox.Show(GetFormattedStringResource("messageUnexpectedErrorPrefix", ex.Message), GetStringResource("titleError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsOperationInProgress = false;
                ProgressValue = 0;
                StatusMessage = GetStringResource("statusOperationFinished");
            }
        }

        [RelayCommand]
        private async Task TerminateProcessesAsync()
        {
            if (IsOperationInProgress) return;
            try
            {
                IsOperationInProgress = true;
                StatusMessage = GetStringResource("statusTerminatingProcesses");

                var result = await _blitzService.TerminateProcessesAsync();
                var messageToShow = GetStringResource(result.Message);

                if (result.IsSuccess)
                {
                    StatusMessage = messageToShow;
                    MessageBox.Show(messageToShow, GetStringResource("titleSuccess"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = GetStringResource("statusFailedToTerminateProcessesGeneral");
                    MessageBox.Show(GetFormattedStringResource("messageErrorPrefix", messageToShow), GetStringResource("titleError"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating processes");
                StatusMessage = GetStringResource("statusErrorTerminatingProcesses");
            }
            finally
            {
                IsOperationInProgress = false;
            }
        }

        [RelayCommand]
        private async Task ClearCacheAsync()
        {
            if (IsOperationInProgress) return;
            try
            {
                IsOperationInProgress = true;
                StatusMessage = GetStringResource("statusClearingCache");

                var result = await _blitzService.ClearCacheAsync();
                var messageToShow = GetStringResource(result.Message);

                if (result.IsSuccess)
                {
                    StatusMessage = messageToShow;
                    MessageBox.Show(messageToShow, GetStringResource("titleSuccess"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = GetStringResource("statusFailedToClearCacheGeneral");
                    MessageBox.Show(GetFormattedStringResource("messageErrorPrefix", messageToShow), GetStringResource("titleError"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                StatusMessage = GetStringResource("statusErrorClearingCache");
            }
            finally
            {
                IsOperationInProgress = false;
            }
        }

        [RelayCommand]
        private async Task UninstallAsync()
        {
            if (IsOperationInProgress) return;
            var confirmResult = MessageBox.Show(
                GetStringResource("promptConfirmUninstallMessage"),
                GetStringResource("titleConfirmUninstall"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes) return;

            try
            {
                IsOperationInProgress = true;
                StatusMessage = GetStringResource("statusUninstallingBlitzProgress");

                var uninstallResult = await _blitzService.UninstallAsync();
                var messageToShow = GetStringResource(uninstallResult.Message);


                if (uninstallResult.IsSuccess)
                {
                    StatusMessage = messageToShow;
                    MessageBox.Show(messageToShow, GetStringResource("titleSuccess"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await RefreshBlitzInstallationAsync();
                }
                else
                {
                    StatusMessage = GetStringResource("statusErrorUninstallingBlitz");
                    MessageBox.Show(GetFormattedStringResource("messageErrorPrefix", messageToShow), GetStringResource("titleError"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uninstalling Blitz");
                StatusMessage = GetStringResource("statusErrorUninstallingBlitz");
            }
            finally
            {
                IsOperationInProgress = false;
            }
        }

        [RelayCommand]
        private async Task DownloadPortableAsync()
        {
            if (IsOperationInProgress) return;
            try
            {
                IsOperationInProgress = true;
                ProgressValue = 0;
                StatusMessage = GetStringResource("statusDownloadingPortable");

                var downloadProgress = new Progress<double>(progress => ProgressValue = progress);
                var result = await _blitzService.DownloadPortableAsync(downloadProgress);
                var messageToShow = GetStringResource(result.Message);

                if (result.IsSuccess)
                {
                    StatusMessage = messageToShow;
                    MessageBox.Show(messageToShow, GetStringResource("titleSuccess"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = GetStringResource("statusFailedToDownloadPortableGeneral");
                    MessageBox.Show(GetFormattedStringResource("messageErrorPrefix", messageToShow), GetStringResource("titleError"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading portable Blitz");
                StatusMessage = GetStringResource("statusErrorDownloadingPortable");
            }
            finally
            {
                IsOperationInProgress = false;
                ProgressValue = 0;
                StatusMessage = GetStringResource("statusOperationFinished");
            }
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            ApplyTheme();
            OnPropertyChanged(nameof(NextThemeDisplayName));
        }

        private void ApplyTheme()
        {
            if (Application.Current == null) return;
            var themeDictionaries = Application.Current.Resources.MergedDictionaries;
            if (themeDictionaries == null) return;

            var oldThemes = themeDictionaries.Where(
                dict => dict.Source != null &&
                        (dict.Source.OriginalString.EndsWith("DarkTheme.xaml") ||
                         dict.Source.OriginalString.EndsWith("BlueTheme.xaml"))
            ).ToList();

            foreach (var oldTheme in oldThemes)
            {
                themeDictionaries.Remove(oldTheme);
            }

            string themeUriString = IsDarkTheme ? "Themes/DarkTheme.xaml" : "Themes/BlueTheme.xaml";
            try
            {
                var newTheme = new ResourceDictionary { Source = new Uri(themeUriString, UriKind.RelativeOrAbsolute) };
                themeDictionaries.Add(newTheme);
                _logger.LogInformation("Theme changed to: {ThemeName}", IsDarkTheme ? GetStringResource("themeNameDark") : GetStringResource("themeNameBlue"));
                StatusMessage = GetFormattedStringResource("statusThemeSetTo", IsDarkTheme ? GetStringResource("themeNameDark") : GetStringResource("themeNameBlue"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply theme: {ThemeUri}", themeUriString);
                StatusMessage = GetFormattedStringResource("statusErrorApplyingTheme", themeUriString);
            }
        }
    }
}