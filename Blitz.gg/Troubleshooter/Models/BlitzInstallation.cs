namespace BlitzTroubleshooter.Models;

public enum BlitzInstallationStatus
{
    NotInstalled,
    Installed,
    Corrupted
}

public record BlitzInstallation(
    BlitzInstallationStatus Status,
    string? InstallPath,
    string? Version,
    List<string> DetectedPaths,
    List<string> MissingExecutables)
{
    public bool IsInstalled => Status == BlitzInstallationStatus.Installed;
    public bool IsCorrupted => Status == BlitzInstallationStatus.Corrupted;
    public bool HasAnyFiles => DetectedPaths.Any();
}

public record GameInfo(
    string Name,
    string DisplayName,
    bool IsInstalled,
    string? InstallPath);

public record TroubleshootResult(
    bool IsSuccess,
    string Message,
    Exception? Exception = null)
{
    public static TroubleshootResult Success(string message = "Operation completed successfully") =>
        new(true, message);

    public static TroubleshootResult Failure(string message, Exception? exception = null) =>
        new(false, message, exception);
}
