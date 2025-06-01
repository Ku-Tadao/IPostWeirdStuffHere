using System;
using System.Collections.Generic;
using System.Linq;

namespace BlitzTroubleshooter.Models
{
    public enum BlitzInstallationStatus
    {
        NotInstalled,
        Installed,
        Corrupted
    }

    public class BlitzInstallation
    {
        public BlitzInstallationStatus Status { get; }
        public string InstallPath { get; }
        public string Version { get; }
        public List<string> DetectedPaths { get; }
        public List<string> MissingExecutables { get; }

        public BlitzInstallation(
            BlitzInstallationStatus status,
            string installPath,
            string version,
            List<string> detectedPaths,
            List<string> missingExecutables)
        {
            Status = status;
            InstallPath = installPath;
            Version = version;
            DetectedPaths = detectedPaths ?? new List<string>();
            MissingExecutables = missingExecutables ?? new List<string>();
        }

        public bool IsInstalled => Status == BlitzInstallationStatus.Installed;
        public bool IsCorrupted => Status == BlitzInstallationStatus.Corrupted;
        public bool HasAnyFiles => DetectedPaths.Any();
    }

    public class GameInfo
    {
        public string Name { get; }
        public string DisplayName { get; }
        public bool IsInstalled { get; }
        public string InstallPath { get; }

        public GameInfo(
            string name,
            string displayName,
            bool isInstalled,
            string installPath)
        {
            Name = name;
            DisplayName = displayName;
            IsInstalled = isInstalled;
            InstallPath = installPath;
        }
    }

    public class TroubleshootResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public Exception Exception { get; }

        public TroubleshootResult(
            bool isSuccess,
            string message,
            Exception exception = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Exception = exception;
        }

        public static TroubleshootResult Success(string message = "Operation completed successfully") =>
            new TroubleshootResult(true, message, null); // Explicitly pass null for exception

        public static TroubleshootResult Failure(string message, Exception exception = null) =>
            new TroubleshootResult(false, message, exception);
    }
}
