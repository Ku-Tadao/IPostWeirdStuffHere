using BlitzTroubleshooter.Configuration;
using BlitzTroubleshooter.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace BlitzTroubleshooter.Services;

public class BlitzService : IBlitzService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlitzService> _logger;
    private readonly BlitzConfiguration _config;

    private static readonly Dictionary<string, string[]> GameProcesses = new()
    {
        ["League of Legends"] = new[] { "League of Legends", "LeagueClient" },
        ["VALORANT"] = new[] { "VALORANT-Win64-Shipping" },
        ["Marvel Rivals"] = new[] { "MarvelRivals", "MarvelRivals-Win64-Shipping" },
        ["Counter-Strike 2"] = new[] { "cs2", "csgo" },
        ["Apex Legends"] = new[] { "r5apex" },
        ["Escape From Tarkov"] = new[] { "EscapeFromTarkov" },
        ["Fortnite"] = new[] { "FortniteClient-Win64-Shipping" },
        ["Deadlock"] = new[] { "deadlock" },
        ["Minecraft"] = new[] { "javaw", "MinecraftLauncher" }
    };

    public BlitzService(
        HttpClient httpClient,
        ILogger<BlitzService> logger,
        IOptions<BlitzConfiguration> config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<BlitzInstallation> GetInstallationInfoAsync()
    {
        _logger.LogInformation("Detecting Blitz installation");

        var detectedPaths = new List<string>();
        var missingExecutables = new List<string>();
        string? primaryInstallPath = null;
        string? version = null;
        var hasMainExecutable = false;

        var registryInstallPath = GetInstallPathFromRegistry();
        if (!string.IsNullOrEmpty(registryInstallPath))
        {
            detectedPaths.Add(registryInstallPath);
            primaryInstallPath = registryInstallPath;
        }

        foreach (var path in _config.InstallationPaths)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(path);
            if (Directory.Exists(expandedPath))
            {
                detectedPaths.Add(expandedPath);
                primaryInstallPath ??= expandedPath;

                version ??= await GetVersionFromPath(expandedPath);
            }
        }

        var expectedMainPath = Environment.ExpandEnvironmentVariables("%localappdata%\\Programs\\Blitz");
        var mainExecutablePath = Path.Combine(expectedMainPath, "Blitz.exe");

        if (File.Exists(mainExecutablePath))
        {
            hasMainExecutable = true;
            if (!detectedPaths.Contains(expectedMainPath))
            {
                detectedPaths.Add(expectedMainPath);
            }
            primaryInstallPath ??= expectedMainPath;
            version ??= await GetVersionFromPath(expectedMainPath);
        }
        else if (Directory.Exists(expectedMainPath))
        {
            missingExecutables.Add(mainExecutablePath);
            if (!detectedPaths.Contains(expectedMainPath))
            {
                detectedPaths.Add(expectedMainPath);
            }
        }

        var alternativeExecutables = new[]
        {
            Path.Combine(expectedMainPath, "blitz.exe"),
            Path.Combine(expectedMainPath, "BlitzDesktop.exe"),
            Path.Combine(Environment.ExpandEnvironmentVariables("%localappdata%\\Programs\\blitz-core"), "blitz-core.exe"),
            Path.Combine(Environment.ExpandEnvironmentVariables("%localappdata%\\blitz-updater"), "blitz-updater.exe")
        };

        foreach (var execPath in alternativeExecutables)
        {
            if (File.Exists(execPath))
            {
                hasMainExecutable = true;
                var parentDir = Path.GetDirectoryName(execPath);
                if (parentDir != null && !detectedPaths.Contains(parentDir))
                {
                    detectedPaths.Add(parentDir);
                }
            }
            else
            {
                var parentDir = Path.GetDirectoryName(execPath);
                if (parentDir != null && Directory.Exists(parentDir))
                {
                    missingExecutables.Add(execPath);
                }
            }
        }

        BlitzInstallationStatus status;
        if (!detectedPaths.Any())
        {
            status = BlitzInstallationStatus.NotInstalled;
            _logger.LogInformation("Blitz installation: Not installed");
        }
        else if (hasMainExecutable)
        {
            status = BlitzInstallationStatus.Installed;
            _logger.LogInformation("Blitz installation: Installed at {Path}", primaryInstallPath);
        }
        else
        {
            status = BlitzInstallationStatus.Corrupted;
            _logger.LogWarning("Blitz installation: Corrupted - files exist but main executable missing");
            _logger.LogWarning("Missing executables: {MissingExes}", string.Join(", ", missingExecutables));
        }

        return new BlitzInstallation(status, primaryInstallPath, version, detectedPaths, missingExecutables);
    }

    public async Task<TroubleshootResult> TerminateProcessesAsync()
    {
        _logger.LogInformation("Terminating Blitz processes");
        try
        {
            var terminatedCount = 0;
            foreach (var processName in _config.ProcessNames)
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        await process.WaitForExitAsync();
                        terminatedCount++;
                        _logger.LogInformation("Terminated process: {ProcessName} (PID: {ProcessId})", process.ProcessName, process.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to terminate process: {ProcessName}", processName);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }

            _logger.LogInformation("Terminated {Count} processes", terminatedCount);
            return TroubleshootResult.Success($"serviceTerminatedProcessesCount|{terminatedCount}"); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating Blitz processes");
            return TroubleshootResult.Failure("serviceFailedToTerminateProcesses"); 
        }
    }

    public async Task<TroubleshootResult> UninstallAsync()
    {
        _logger.LogInformation("Uninstalling Blitz");
        try
        {
            var installation = await GetInstallationInfoAsync();
            if (installation.Status == BlitzInstallationStatus.NotInstalled)
            {
                return TroubleshootResult.Success("serviceBlitzIsNotInstalledMessage"); 
            }

            _logger.LogInformation("Terminating Blitz processes before uninstall");
            var terminateResult = await TerminateProcessesAsync();
            if (!terminateResult.IsSuccess)
            {
                _logger.LogWarning("Failed to terminate processes: {MessageKey}", terminateResult.Message);
            }
            await Task.Delay(2000);

            var deletedCount = 0;
            var errors = new List<string>();
            foreach (var path in installation.DetectedPaths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                        deletedCount++;
                        _logger.LogInformation("Deleted directory: {Path}", path);
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Failed to delete {path}: {ex.Message}";
                    errors.Add(error);
                    _logger.LogWarning(ex, "Failed to delete directory: {Path}", path);
                }
            }

            var cacheResult = await ClearCacheAsync();
            if (!cacheResult.IsSuccess)
            {

                errors.Add($"Cache cleanup error (key: {cacheResult.Message})");
            }

            var installationTypeKey = installation.IsCorrupted ? "termCorruptedInstallation" : "termInstallation";

            if (errors.Any())
            {
                _logger.LogError("Partial uninstall. Errors: {Errors}", string.Join("; ", errors));

                return TroubleshootResult.Failure("serviceFailedToUninstallBlitzMessage");
            }
            _logger.LogInformation("Successfully removed {InstallationType} ({Count} dirs)", installationTypeKey, deletedCount);
            return TroubleshootResult.Success("serviceSuccessfullyRemovedBlitz"); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uninstalling Blitz");
            return TroubleshootResult.Failure("serviceFailedToUninstallBlitzMessage"); 
        }
    }

    public async Task<TroubleshootResult> ClearCacheAsync()
    {
        _logger.LogInformation("Clearing Blitz cache");
        try
        {
            var deletedCount = 0;
            var failedCount = 0;
            var foldersToDelete = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Blitz"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "gg.blitz"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Blitz"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "gg.blitz"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "blitz-core"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "blitz-core"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Blitz-helpers")
            };

            // Helper function to attempt deletion
            async Task<bool> TryDeleteFoldersAsync()
            {
                var anyFailed = false;
                foreach (var folderPath in foldersToDelete)
                {
                    if (Directory.Exists(folderPath))
                    {
                        try
                        {
                            Directory.Delete(folderPath, true);
                            deletedCount++;
                            _logger.LogInformation("Cleared cache directory: {Path}", folderPath);
                        }
                        catch (Exception ex)
                        {
                            anyFailed = true;
                            _logger.LogWarning(ex, "Failed to delete cache directory: {Path}", folderPath);
                        }
                    }
                }
                return !anyFailed;
            }

            // First attempt
            if (!await TryDeleteFoldersAsync())
            {
                _logger.LogInformation("Initial cache clear failed. Attempting to terminate processes and retry.");
                
                // Try to kill processes if deletion failed
                await TerminateProcessesAsync();
                await Task.Delay(1000); // Give OS time to release locks

                // Reset counts for retry (only count successful deletions once)
                // Actually, we should only retry the ones that exist. 
                // The helper checks Directory.Exists, so if it was deleted in first pass, it won't be retried.
                // But deletedCount would be double counted if we don't be careful.
                // Let's just reset deletedCount and try again? No, some might be gone.
                // We can just call it again. The Exists check handles it.
                // But deletedCount will increment again? No, because Exists will be false.
                // Wait, if I deleted it, Exists is false. So deletedCount won't increment.
                // Correct.

                if (!await TryDeleteFoldersAsync())
                {
                    failedCount++; // Mark as failed if second attempt also fails
                }
            }

            _logger.LogInformation("Cache clear operation: {Count} folders deleted. {Failed} failed.", deletedCount, failedCount);

            if (failedCount > 0)
            {
                return TroubleshootResult.Failure("serviceCacheClearFailedLocked");
            }

            return deletedCount > 0
                ? TroubleshootResult.Success($"serviceCacheClearedSuccessfullyCount|{deletedCount}")
                : TroubleshootResult.Success("serviceNoCacheFoldersFound");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing Blitz cache");
            return TroubleshootResult.Failure("serviceFailedToClearCacheMessage"); 
        }
    }

    public async Task<TroubleshootResult> DownloadAndInstallAsync(
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading and installing Blitz");
        try
        {
            var tempPath = Path.GetTempPath();
            var tempFilePath = Path.Combine(tempPath, "BlitzInstaller.exe");

            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); _logger.LogInformation("Deleted existing temp file"); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete existing temp file: {Path}", tempFilePath); tempFilePath = Path.Combine(tempPath, $"BlitzInstaller_{DateTime.Now.Ticks}.exe"); }
            }
            _logger.LogInformation("Downloading Blitz installer to: {Path}", tempFilePath);

            using (var response = await _httpClient.GetAsync(_config.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0; int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        totalRead += bytesRead;
                        if (totalBytes > 0) progress?.Report((double)totalRead / totalBytes * 100);
                    }
                    await fileStream.FlushAsync(cancellationToken);
                }
            }
            _logger.LogInformation("Download completed. File size: {Size} bytes", new FileInfo(tempFilePath).Length);
            await Task.Delay(500, cancellationToken);
            if (!File.Exists(tempFilePath)) throw new FileNotFoundException("Downloaded file not found", tempFilePath);
            if (new FileInfo(tempFilePath).Length == 0) throw new InvalidOperationException("Downloaded file is empty");

            _logger.LogInformation("Launching installer: {Path}", tempFilePath);
            var startInfo = new ProcessStartInfo { FileName = tempFilePath, UseShellExecute = true, Verb = "runas", WorkingDirectory = Path.GetDirectoryName(tempFilePath) };
            try
            {
                var process = Process.Start(startInfo);
                if (process == null) throw new InvalidOperationException("Failed to start installer process");
                _logger.LogInformation("Blitz installer launched successfully with PID: {ProcessId}", process.Id);
                return TroubleshootResult.Success("serviceInstallerLaunched");
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                _logger.LogInformation("User cancelled UAC prompt for installer");
                return TroubleshootResult.Failure("serviceInstallationCancelledByUser"); 
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading and installing Blitz");
            return TroubleshootResult.Failure($"serviceFailedToDownloadInstallFull|{ex.Message}");
        }
    }

    public async Task<TroubleshootResult> DownloadPortableAsync(
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading portable Blitz");
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var blitzPortablePath = Path.Combine(localAppData, "Programs", "Blitz");
            var zipPath = Path.Combine(blitzPortablePath, "blitzportable.zip");

            if (Directory.Exists(blitzPortablePath))
            {
                try { Directory.Delete(blitzPortablePath, true); _logger.LogInformation("Cleaned up existing portable installation"); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to clean up existing portable installation"); }
            }
            Directory.CreateDirectory(blitzPortablePath);
            _logger.LogInformation("Downloading portable Blitz to: {Path}", zipPath);

            using (var response = await _httpClient.GetAsync(_config.PortableDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0; int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        totalRead += bytesRead;
                        if (totalBytes > 0) progress?.Report((double)totalRead / totalBytes * 100);
                    }
                    await fileStream.FlushAsync(cancellationToken);
                }
            }
            _logger.LogInformation("Portable download completed. File size: {Size} bytes", new FileInfo(zipPath).Length);
            await Task.Delay(1500, cancellationToken);
            if (!File.Exists(zipPath)) throw new FileNotFoundException("Downloaded zip file not found", zipPath);
            await Task.Run(() => { using var testStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read); });

            _logger.LogInformation("Extracting portable Blitz...");
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try { ZipFile.ExtractToDirectory(zipPath, blitzPortablePath, overwriteFiles: true); _logger.LogInformation("Extraction completed successfully on attempt {Attempt}", attempt); break; }
                catch (IOException ex) when (attempt < 3) { _logger.LogWarning("Extraction attempt {Attempt} failed: {Message}. Retrying...", attempt, ex.Message); await Task.Delay(2000 * attempt, cancellationToken); }
            }
            try { File.Delete(zipPath); _logger.LogInformation("Cleaned up zip file"); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete zip file"); }

            var possibleExeNames = new[] { "blitz.exe", "Blitz.exe", "BlitzDesktop.exe" };
            var launchedSuccessfully = false;
            foreach (var exeName in possibleExeNames)
            {
                var blitzExePath = Path.Combine(blitzPortablePath, exeName);
                if (File.Exists(blitzExePath))
                {
                    try { Process.Start(new ProcessStartInfo { FileName = blitzExePath, UseShellExecute = true, WorkingDirectory = blitzPortablePath }); _logger.LogInformation("Launched portable Blitz: {Path}", blitzExePath); launchedSuccessfully = true; break; }
                    catch (Exception ex) { _logger.LogWarning(ex, "Failed to launch Blitz executable: {Path}", blitzExePath); }
                }
            }
            if (!launchedSuccessfully)
            {
                var allExeFiles = Directory.GetFiles(blitzPortablePath, "*.exe", SearchOption.AllDirectories);
                var exeList = string.Join(", ", allExeFiles.Select(Path.GetFileName));
                _logger.LogWarning("Could not find standard Blitz executable. Available executables: {ExeFiles}", exeList);

                return TroubleshootResult.Success("servicePortableDownloadedExtractedLaunchManuallyKey"); 
            }
            return TroubleshootResult.Success("servicePortableDownloadedExtractedLaunched");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading portable Blitz");
            return TroubleshootResult.Failure($"serviceFailedToDownloadPortableFull|{ex.Message}");
        }
    }


    public async Task<TroubleshootResult> FixAllIssuesAsync(
        IProgress<string>? statusProgress = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        return await FixAllIssuesAsync(statusProgress, downloadProgress, false, cancellationToken);
    }

    public async Task<TroubleshootResult> FixAllIssuesForceAsync(
        IProgress<string>? statusProgress = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        return await FixAllIssuesAsync(statusProgress, downloadProgress, true, cancellationToken);
    }

    private async Task<TroubleshootResult> FixAllIssuesAsync(
        IProgress<string>? statusProgress,
        IProgress<double>? downloadProgress,
        bool skipGameCheck,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting comprehensive Blitz troubleshooting (skipGameCheck: {SkipGameCheck})", skipGameCheck);
        try
        {
            if (!skipGameCheck)
            {
                statusProgress?.Report("statusCheckingRunningGames"); 
                var runningGames = await GetRunningGamesAsync();
                if (runningGames.Any())
                {
                    var gamesList = string.Join(", ", runningGames);
                    _logger.LogWarning("Running games detected: {Games}", gamesList);
                    
                    return TroubleshootResult.Failure($"RUNNING_GAMES_DETECTED_KEY:{gamesList}");
                }
            }

            statusProgress?.Report("statusTerminatingProcesses"); 
            var terminateResult = await TerminateProcessesAsync();
            if (!terminateResult.IsSuccess) return terminateResult;

            await Task.Delay(2000, cancellationToken);

            statusProgress?.Report("statusUninstallingExistingBlitz"); 
            var uninstallResult = await UninstallAsync();
            if (!uninstallResult.IsSuccess)
            {
                _logger.LogWarning("Uninstall failed but proceeding with installation (Best Effort): {Message}", uninstallResult.Message);
            }
            // Proceed even if uninstall fails (Best Effort)

            statusProgress?.Report("statusDownloadingInstaller");
            var downloadResult = await DownloadAndInstallAsync(downloadProgress, cancellationToken);
            if (!downloadResult.IsSuccess) return downloadResult; 

            statusProgress?.Report("statusTroubleshootingComplete"); 
            var successMessageKey = skipGameCheck ? "serviceFixAllSuccessForcedMessage" : "serviceFixAllSuccessMessage";
            return TroubleshootResult.Success(successMessageKey); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during comprehensive troubleshooting");
            return TroubleshootResult.Failure("serviceFailedToFixAllIssues"); 
        }
    }

    public async Task<List<string>> GetRunningGamesAsync()
    {
        _logger.LogInformation("Checking for running games");
        return await Task.Run(() =>
        {
            var runningGames = new List<string>();
            foreach (var (gameName, processNames) in GameProcesses)
            {
                foreach (var processName in processNames)
                {
                    try
                    {
                        var processes = Process.GetProcessesByName(processName);
                        if (processes.Length > 0)
                        {
                            runningGames.Add(gameName);
                            _logger.LogInformation("Found running game: {GameName} (Process: {ProcessName})", gameName, processName);
                            foreach (var process in processes) process.Dispose();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error checking process: {ProcessName}", processName);
                    }
                }
            }
            _logger.LogInformation("Found {Count} running games", runningGames.Count);
            return runningGames;
        });
    }

    private string? GetInstallPathFromRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey?.GetValue("DisplayName") is string displayName &&
                        displayName.Contains("Blitz", StringComparison.OrdinalIgnoreCase))
                    {
                        return subKey.GetValue("InstallLocation") as string;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read registry for Blitz installation");
        }
        return null;
    }

    private async Task<string?> GetVersionFromPath(string path)
    {
        try
        {
            var blitzExePath = Path.Combine(path, "blitz.exe");
            if (File.Exists(blitzExePath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(blitzExePath);
                return versionInfo.FileVersion;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get version from path: {Path}", path);
        }
        return null;
    }
}