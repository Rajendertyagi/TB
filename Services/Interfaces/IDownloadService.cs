using TB.Services.Downloads;

namespace TB.Services.Interfaces;

public interface IDownloadService
{
    IReadOnlyList<DownloadItem> Downloads { get; }
    event Action<DownloadItem>? OnProgress;
    event Action<DownloadItem>? OnCompleted;
    event Action<DownloadItem>? OnFailed;
    DownloadItem AddDownload(string url, string name, string filePath, long totalBytes);
    void UpdateProgress(int id, long receivedBytes);
    void CompleteDownload(int id);
    void FailDownload(int id, string error);
    void RemoveDownload(int id);
    void ClearAll();
}
