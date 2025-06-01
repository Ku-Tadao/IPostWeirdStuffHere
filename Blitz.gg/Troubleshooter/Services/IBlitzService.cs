using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlitzTroubleshooter.Models;

namespace BlitzTroubleshooter.Services
{
    public interface IBlitzService
    {
        Task<BlitzInstallation> GetInstallationInfoAsync();
        Task<TroubleshootResult> TerminateProcessesAsync();
        Task<TroubleshootResult> UninstallAsync();
        Task<TroubleshootResult> ClearCacheAsync();
        Task<TroubleshootResult> DownloadAndInstallAsync(IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TroubleshootResult> DownloadPortableAsync(IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TroubleshootResult> FixAllIssuesAsync(IProgress<string> statusProgress = null, IProgress<double> downloadProgress = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TroubleshootResult> FixAllIssuesForceAsync(IProgress<string> statusProgress = null, IProgress<double> downloadProgress = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<string>> GetRunningGamesAsync();
    }
}
