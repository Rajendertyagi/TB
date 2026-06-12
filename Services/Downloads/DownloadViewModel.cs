using System;
using System.Text.Json.Serialization;

namespace TB.Services.Downloads;

public class DownloadViewModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("filePath")] public string FilePath { get; set; } = "";
    [JsonPropertyName("totalBytes")] public long TotalBytes { get; set; }
    [JsonPropertyName("receivedBytes")] public long ReceivedBytes { get; set; }
    [JsonPropertyName("progressPercent")] public double ProgressPercent { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("date")] public long Date { get; set; }

    public static DownloadViewModel FromItem(DownloadItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Url = item.Url,
        FilePath = item.FilePath,
        TotalBytes = item.TotalBytes,
        ReceivedBytes = item.ReceivedBytes,
        ProgressPercent = item.ProgressPercent,
        Status = item.Status,
        Date = new DateTimeOffset(item.Date).ToUnixTimeMilliseconds()
    };
}
