namespace BlitzTroubleshooter.Configuration;

public class BlitzConfiguration
{
    public string DownloadUrl { get; set; } = string.Empty;
    public string PortableDownloadUrl { get; set; } = string.Empty;
    public List<string> SupportedGames { get; set; } = new();
    public List<string> InstallationPaths { get; set; } = new();
    public List<string> ProcessNames { get; set; } = new();
}
