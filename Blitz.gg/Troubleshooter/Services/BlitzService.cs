using BlitzTroubleshooter.Configuration;
using BlitzTroubleshooter.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Linq; // Added for Enumerable.Any()
using System.Threading; // Added for CancellationToken
using System.Threading.Tasks; // Added for Task

namespace BlitzTroubleshooter.Services
{
public class BlitzService : IBlitzService
{
    private readonly HttpClient _httpClient;
    // private readonly ILogger<BlitzService> _logger; // Removed
    private readonly BlitzConfiguration _config;


    private static readonly Dictionary<string, string[]> GameProcesses = new Dictionary<string, string[]>()
    {
        {"League of Legends", new[] { "League of Legends", "LeagueClient" }},
        {"VALORANT", new[] { "VALORANT-Win64-Shipping" }},
        {"Marvel Rivals", new[] { "MarvelRivals", "MarvelRivals-Win64-Shipping" }},
        {"Counter-Strike 2", new[] { "cs2", "csgo" }},
        {"Apex Legends", new[] { "r5apex" }},
        {"Escape From Tarkov", new[] { "EscapeFromTarkov" }},
        {"Fortnite", new[] { "FortniteClient-Win64-Shipping" }},
        {"Deadlock", new[] { "deadlock" }},
        {"Minecraft", new[] { "javaw", "MinecraftLauncher" }}
    };

    public BlitzService(
        HttpClient httpClient,
        BlitzConfiguration config
        )
    {
        _httpClient = httpClient ?? new HttpClient();
        _config = config ?? new BlitzConfiguration(); // Ensure config is not null, provide defaults if necessary
    }

    public async Task<BlitzInstallation> GetInstallationInfoAsync()
    {
        // _logger.LogInformation("Detecting Blitz installation");

        var detectedPaths = new List<string>();
        var missingExecutables = new List<string>();
        string primaryInstallPath = null;
        string version = null;
        var hasMainExecutable = false;

        var registryInstallPath = GetInstallPathFromRegistry();
        if (!string.IsNullOrEmpty(registryInstallPath))
        {
            detectedPaths.Add(registryInstallPath);
            primaryInstallPath = registryInstallPath;
        }

        if (_config.InstallationPaths != null) // Check if config paths exist
        {
            foreach (var path in _config.InstallationPaths)
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(path);
                if (Directory.Exists(expandedPath))
                {
                    detectedPaths.Add(expandedPath);
                    if (primaryInstallPath == null) primaryInstallPath = expandedPath;

                    if (version == null) version = await GetVersionFromPath(expandedPath);
                }
            }
        }

        var expectedMainPath = Environment.ExpandEnvironmentVariables("%LocalAppData%\\Programs\\Blitz"); // Corrected casing
        var mainExecutablePath = Path.Combine(expectedMainPath, "Blitz.exe");

        if (File.Exists(mainExecutablePath))
        {
            hasMainExecutable = true;
            if (!detectedPaths.Contains(expectedMainPath))
            {
                detectedPaths.Add(expectedMainPath);
            }
            if (primaryInstallPath == null) primaryInstallPath = expectedMainPath;
            if (version == null) version = await GetVersionFromPath(expectedMainPath);
        }
        else if (Directory.Exists(expectedMainPath)) // If directory exists but Blitz.exe is missing
        {
            missingExecutables.Add(mainExecutablePath);
            if (!detectedPaths.Contains(expectedMainPath)) // Still add path to detected if not already there
            {
                detectedPaths.Add(expectedMainPath);
            }
        }

        var alternativeExecutables = new[]
        {
            Path.Combine(expectedMainPath, "blitz.exe"), // Redundant if mainExecutablePath is this
            Path.Combine(expectedMainPath, "BlitzDesktop.exe"),
            Path.Combine(Environment.ExpandEnvironmentVariables("%LocalAppData%\\Programs\\blitz-core"), "blitz-core.exe"),
            Path.Combine(Environment.ExpandEnvironmentVariables("%LocalAppData%\\blitz-updater"), "blitz-updater.exe")
        };

        foreach (var execPath in alternativeExecutables)
        {
            if (File.Exists(execPath))
            {
                hasMainExecutable = true; // If any executable is found, consider it as having main executable for status purposes
                var parentDir = Path.GetDirectoryName(execPath);
                if (parentDir != null && !detectedPaths.Contains(parentDir))
                {
                    detectedPaths.Add(parentDir);
                }
            }
            else // If alternative executable is missing
            {
                var parentDir = Path.GetDirectoryName(execPath);
                // Only add to missing if its parent directory actually exists (meaning it *should* be there)
                if (parentDir != null && Directory.Exists(parentDir))
                {
                    missingExecutables.Add(execPath);
                }
            }
        }

        // Deduplicate missing executables
        missingExecutables = missingExecutables.Distinct().ToList();


        BlitzInstallationStatus status;
        if (!detectedPaths.Any()) // No files or folders detected at all
        {
            status = BlitzInstallationStatus.NotInstalled;
            // _logger.LogInformation("Blitz installation: Not installed");
        }
        else if (hasMainExecutable) // Some executable found in expected locations
        {
            status = BlitzInstallationStatus.Installed;
            // _logger.LogInformation("Blitz installation: Installed at {Path}", primaryInstallPath);
        }
        else // Directories/files found, but no main executable
        {
            status = BlitzInstallationStatus.Corrupted;
            // _logger.LogWarning("Blitz installation: Corrupted - files exist but main executable missing");
            // _logger.LogWarning("Missing executables: {MissingExes}", string.Join(", ", missingExecutables));
        }

        return new BlitzInstallation(status, primaryInstallPath, version, detectedPaths, missingExecutables);
    }

    public async Task<TroubleshootResult> TerminateProcessesAsync()
    {
        // _logger.LogInformation("Terminating Blitz processes");
        try
        {
            var terminatedCount = 0;
            if (_config.ProcessNames != null)
            {
                foreach (var processName in _config.ProcessNames)
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            // await process.WaitForExitAsync(); // WaitForExitAsync is not in .NET Framework 4.8
                            process.WaitForExit(); // Use synchronous version
                            terminatedCount++;
                            // _logger.LogInformation("Terminated process: {ProcessName} (PID: {ProcessId})", process.ProcessName, process.Id);
                        }
                        catch (Exception /*ex*/)
                        {
                            // _logger.LogWarning(ex, "Failed to terminate process: {ProcessName}", processName);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            // _logger.LogInformation("Terminated {Count} processes", terminatedCount);
            return TroubleshootResult.Success("serviceTerminatedProcessesCount"); 
        }
        catch (Exception /*ex*/)
        {
            // _logger.LogError(ex, "Error terminating Blitz processes");
            return TroubleshootResult.Failure("serviceFailedToTerminateProcesses"); 
        }
    }

    public async Task<TroubleshootResult> UninstallAsync()
    {
        // _logger.LogInformation("Uninstalling Blitz");
        try
        {
            var installation = await GetInstallationInfoAsync();
            if (installation.Status == BlitzInstallationStatus.NotInstalled)
            {
                return TroubleshootResult.Success("serviceBlitzIsNotInstalledMessage"); 
            }

            // _logger.LogInformation("Terminating Blitz processes before uninstall");
            var terminateResult = await TerminateProcessesAsync();
            if (!terminateResult.IsSuccess)
            {
                // _logger.LogWarning("Failed to terminate processes: {MessageKey}", terminateResult.Message);
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
                        // _logger.LogInformation("Deleted directory: {Path}", path);
                    }
                }
                catch (Exception ex)
                {
                    var error = string.Format("Failed to delete {0}: {1}", path, ex.Message);
                    errors.Add(error);
                    // _logger.LogWarning(ex, "Failed to delete directory: {Path}", path);
                }
            }

            var cacheResult = await ClearCacheAsync();
            if (!cacheResult.IsSuccess)
            {
                errors.Add(string.Format("Cache cleanup error (key: {0})", cacheResult.Message));
            }

            var installationTypeKey = installation.IsCorrupted ? "termCorruptedInstallation" : "termInstallation";

            if (errors.Any())
            {
                // _logger.LogError("Partial uninstall. Errors: {Errors}", string.Join("; ", errors));
                return TroubleshootResult.Failure("serviceFailedToUninstallBlitzMessage");
            }
            // _logger.LogInformation("Successfully removed {InstallationType} ({Count} dirs)", installationTypeKey, deletedCount);
            return TroubleshootResult.Success("serviceSuccessfullyRemovedBlitz"); 
        }
        catch (Exception /*ex*/)
        {
            // _logger.LogError(ex, "Error uninstalling Blitz");
            return TroubleshootResult.Failure("serviceFailedToUninstallBlitzMessage"); 
        }
    }

    public async Task<TroubleshootResult> ClearCacheAsync()
    {
        // _logger.LogInformation("Clearing Blitz cache");
        try
        {
            var deletedCount = 0;
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

            foreach (var folderPath in foldersToDelete)
            {
                if (Directory.Exists(folderPath))
                {
                    try
                    {
                        Directory.Delete(folderPath, true);
                        deletedCount++;
                        // _logger.LogInformation("Cleared cache directory: {Path}", folderPath);
                    }
                    catch (Exception /*ex*/)
                    {
                        // _logger.LogWarning(ex, "Failed to delete cache directory: {Path}", folderPath);
                    }
                }
            }
            // _logger.LogInformation("Cache clear operation: {Count} folders deleted.", deletedCount);
            return deletedCount > 0
                ? TroubleshootResult.Success("serviceCacheClearedSuccessfullyCount")
                : TroubleshootResult.Success("serviceNoCacheFoldersFound");
        }
        catch (Exception /*ex*/)
        {
            // _logger.LogError(ex, "Error clearing Blitz cache");
            return TroubleshootResult.Failure("serviceFailedToClearCacheMessage"); 
        }
    }

    public async Task<TroubleshootResult> DownloadAndInstallAsync(
        IProgress<double> progress = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        // _logger.LogInformation("Downloading and installing Blitz");
        try
        {
            var tempPath = Path.GetTempPath();
            var tempFileName = string.Format("BlitzInstaller_{0}.exe", DateTime.Now.Ticks);
            var tempFilePath = Path.Combine(tempPath, "BlitzInstaller.exe"); // Initial attempt

            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                    // _logger.LogInformation("Deleted existing temp file");
                }
                catch (Exception /*ex*/)
                {
                    // _logger.LogWarning(ex, "Failed to delete existing temp file: {Path}", tempFilePath);
                    tempFilePath = Path.Combine(tempPath, tempFileName); // Use unique name if delete fails
                }
            }
            // _logger.LogInformation("Downloading Blitz installer to: {Path}", tempFilePath);

            if (_config == null || string.IsNullOrEmpty(_config.DownloadUrl))
            {
                // Handle missing configuration for DownloadUrl
                return TroubleshootResult.Failure("serviceFailedToDownloadInstallFullKey_NoUrl");
            }

            using (var response = await _httpClient.GetAsync(_config.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault(-1L);
                using (var contentStream = await response.Content.ReadAsStreamAsync()) // Removed cancellationToken for ReadAsStreamAsync if not available
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0; int bytesRead;
                    // while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0) // C# 7.3 might not have ReadAsync with CancellationToken
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length/*, cancellationToken*/)) > 0)
                    {
                        // await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken); // AsMemory and WriteAsync with Memory<byte> might be newer
                        await fileStream.WriteAsync(buffer, 0, bytesRead/*, cancellationToken*/);
                        totalRead += bytesRead;
                        if (totalBytes > 0 && progress != null) progress.Report((double)totalRead / totalBytes * 100);
                    }
                    await fileStream.FlushAsync(/*cancellationToken*/);
                }
            }
            // _logger.LogInformation("Download completed. File size: {Size} bytes", new FileInfo(tempFilePath).Length);
            await Task.Delay(500, cancellationToken);
            if (!File.Exists(tempFilePath)) throw new FileNotFoundException("Downloaded file not found", tempFilePath);
            if (new FileInfo(tempFilePath).Length == 0) throw new InvalidOperationException("Downloaded file is empty");

            // _logger.LogInformation("Launching installer: {Path}", tempFilePath);
            var startInfo = new ProcessStartInfo { FileName = tempFilePath, UseShellExecute = true, Verb = "runas", WorkingDirectory = Path.GetDirectoryName(tempFilePath) };
            try
            {
                var process = Process.Start(startInfo);
                if (process == null) throw new InvalidOperationException("Failed to start installer process");
                // _logger.LogInformation("Blitz installer launched successfully with PID: {ProcessId}", process.Id);
                return TroubleshootResult.Success("serviceInstallerLaunched");
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // UAC Cancelled
            {
                // _logger.LogInformation("User cancelled UAC prompt for installer");
                return TroubleshootResult.Failure("serviceInstallationCancelledByUser"); 
            }
        }
        catch (Exception /*ex*/)
        {
            // _logger.LogError(ex, "Error downloading and installing Blitz");
            return TroubleshootResult.Failure("serviceFailedToDownloadInstallFullKey"); 
        }
    }

    public async Task<TroubleshootResult> DownloadPortableAsync(
        IProgress<double> progress = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        // _logger.LogInformation("Downloading portable Blitz");
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var blitzPortablePath = Path.Combine(localAppData, "Programs", "BlitzPortable");
            var zipPath = Path.Combine(blitzPortablePath, "blitzportable.zip");

            if (Directory.Exists(blitzPortablePath))
            {
                try
                {
                    Directory.Delete(blitzPortablePath, true);
                    // _logger.LogInformation("Cleaned up existing portable installation");
                }
                catch (Exception /*ex*/)
                {
                    // _logger.LogWarning(ex, "Failed to clean up existing portable installation");
                }
            }
            Directory.CreateDirectory(blitzPortablePath);
            // _logger.LogInformation("Downloading portable Blitz to: {Path}", zipPath);

            if (_config == null || string.IsNullOrEmpty(_config.PortableDownloadUrl))
            {
                // Handle missing configuration for PortableDownloadUrl
                return TroubleshootResult.Failure("serviceFailedToDownloadPortableFullKey_NoUrl");
            }

            using (var response = await _httpClient.GetAsync(_config.PortableDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault(-1L);
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0; int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        if (totalBytes > 0 && progress != null) progress.Report((double)totalRead / totalBytes * 100);
                    }
                    await fileStream.FlushAsync();
                }
            }
            // _logger.LogInformation("Portable download completed. File size: {Size} bytes", new FileInfo(zipPath).Length);
            await Task.Delay(1500, cancellationToken);
            if (!File.Exists(zipPath)) throw new FileNotFoundException("Downloaded zip file not found", zipPath);

            // Test file access before extraction
            await Task.Run(() => { using (var testStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read)) { } });


            // _logger.LogInformation("Extracting portable Blitz...");
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    ZipFile.ExtractToDirectory(zipPath, blitzPortablePath); // Removed overwriteFiles for simplicity, ensure directory is clean or handle specific files
                    // _logger.LogInformation("Extraction completed successfully on attempt {Attempt}", attempt);
                    break;
                }
                catch (IOException /*ex*/) when (attempt < 3)
                {
                    // _logger.LogWarning("Extraction attempt {Attempt} failed: {Message}. Retrying...", attempt, ex.Message);
                    await Task.Delay(2000 * attempt, cancellationToken);
                }
            }
            try
            {
                File.Delete(zipPath);
                // _logger.LogInformation("Cleaned up zip file");
            }
            catch (Exception /*ex*/)
            {
                // _logger.LogWarning(ex, "Failed to delete zip file");
            }

            var possibleExeNames = new[] { "blitz.exe", "Blitz.exe", "BlitzDesktop.exe" };
            var launchedSuccessfully = false;
            foreach (var exeName in possibleExeNames)
            {
                var blitzExePath = Path.Combine(blitzPortablePath, exeName);
                if (File.Exists(blitzExePath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = blitzExePath, UseShellExecute = true, WorkingDirectory = blitzPortablePath });
                        // _logger.LogInformation("Launched portable Blitz: {Path}", blitzExePath);
                        launchedSuccessfully = true;
                        break;
                    }
                    catch (Exception /*ex*/)
                    {
                        // _logger.LogWarning(ex, "Failed to launch Blitz executable: {Path}", blitzExePath);
                    }
                }
            }
            if (!launchedSuccessfully)
            {
                var allExeFiles = Directory.GetFiles(blitzPortablePath, "*.exe", SearchOption.AllDirectories);
                var exeList = string.Join(", ", allExeFiles.Select(Path.GetFileName).ToArray()); // ToArray() for string.Join in .NET Framework
                // _logger.LogWarning("Could not find standard Blitz executable. Available executables: {ExeFiles}", exeList);

                return TroubleshootResult.Success("servicePortableDownloadedExtractedLaunchManuallyKey"); 
            }
            return TroubleshootResult.Success("servicePortableDownloadedExtractedLaunched");
        }
        catch (Exception /*ex*/)
        {
            // _logger.LogError(ex, "Error downloading portable Blitz");
            return TroubleshootResult.Failure("serviceFailedToDownloadPortableFullKey"); 
        }
    }


    public async Task<TroubleshootResult> FixAllIssuesAsync(
        IProgress<string> statusProgress = null,
        IProgress<double> downloadProgress = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await FixAllIssuesAsync(statusProgress, downloadProgress, false, cancellationToken);
    }

    public async Task<TroubleshootResult> FixAllIssuesForceAsync(
        IProgress<string> statusProgress = null,
        IProgress<double> downloadProgress = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await FixAllIssuesAsync(statusProgress, downloadProgress, true, cancellationToken);
    }

    private async Task<TroubleshootResult> FixAllIssuesAsync(
        IProgress<string> statusProgress,
        IProgress<double> downloadProgress,
        bool skipGameCheck,
        CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Starting comprehensive Blitz troubleshooting (skipGameCheck: {SkipGameCheck})", skipGameCheck);
        try
        {
            if (!skipGameCheck)
            {
                if (statusProgress != null) statusProgress.Report("statusCheckingRunningGames");
                var runningGames = await GetRunningGamesAsync();
                if (runningGames.Any())
                {
                    var gamesList = string.Join(", ", runningGames.ToArray());
                    // _logger.LogWarning("Running games detected: {Games}", gamesList);
                    return TroubleshootResult.Failure(string.Format("RUNNING_GAMES_DETECTED_KEY:{0}", gamesList));
                }
            }

            if (statusProgress != null) statusProgress.Report("statusTerminatingProcesses");
            var terminateResult = await TerminateProcessesAsync();
            if (!terminateResult.IsSuccess) return terminateResult;

            await Task.Delay(2000, cancellationToken);

            if (statusProgress != null) statusProgress.Report("statusUninstallingExistingBlitz");
            var uninstallResult = await UninstallAsync();
            // Allow to proceed even if uninstall has minor issues, re-installation might fix them.
            // if (!uninstallResult.IsSuccess) return uninstallResult;

            if (statusProgress != null) statusProgress.Report("statusDownloadingInstaller");
            var downloadResult = await DownloadAndInstallAsync(downloadProgress, cancellationToken);
            if (!downloadResult.IsSuccess) return downloadResult; 

            if (statusProgress != null) statusProgress.Report("statusTroubleshootingComplete");
            var successMessageKey = skipGameCheck ? "serviceFixAllSuccessForcedMessage" : "serviceFixAllSuccessMessage";
            return TroubleshootResult.Success(successMessageKey); 
        }
        catch (Exception /*ex*/)
        {
            // _logger.LogError(ex, "Error during comprehensive troubleshooting");
            return TroubleshootResult.Failure("serviceFailedToFixAllIssues"); 
        }
    }

    public async Task<List<string>> GetRunningGamesAsync()
    {
        // _logger.LogInformation("Checking for running games");
        return await Task.Run(() =>
        {
            var runningGames = new List<string>();
            foreach (var gameEntry in GameProcesses) // Changed iteration to avoid C# 8 deconstruction
            {
                var gameName = gameEntry.Key;
                var processNames = gameEntry.Value;

                foreach (var processName in processNames)
                {
                    try
                    {
                        var processes = Process.GetProcessesByName(processName);
                        if (processes.Length > 0)
                        {
                            runningGames.Add(gameName);
                            // _logger.LogInformation("Found running game: {GameName} (Process: {ProcessName})", gameName, processName);
                            foreach (var process in processes) process.Dispose();
                            break;
                        }
                    }
                    catch (Exception /*ex*/)
                    {
                        // _logger.LogDebug(ex, "Error checking process: {ProcessName}", processName);
                    }
                }
            }
            // _logger.LogInformation("Found {Count} running games", runningGames.Count);
            return runningGames;
        });
    }

    private string GetInstallPathFromRegistry() // Made non-nullable as per original, but be cautious
    {
        try
        {
            // Using var for RegistryKey requires System.IDisposable and a using statement or explicit Dispose()
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey != null && subKey.GetValue("DisplayName") is string displayName &&
                                displayName.Contains("Blitz", StringComparison.OrdinalIgnoreCase))
                            {
                                return subKey.GetValue("InstallLocation") as string;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception /*ex*/)
        {
            // _logger.LogWarning(ex, "Failed to read registry for Blitz installation");
        }
        return null; // Explicitly return null if not found or error
    }

    private async Task<string> GetVersionFromPath(string path) // Made non-nullable, be cautious
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
        catch (Exception /*ex*/)
        {
            // _logger.LogWarning(ex, "Failed to get version from path: {Path}", path);
        }
        return null; // Explicitly return null
    }
}
}