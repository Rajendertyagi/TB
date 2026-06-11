using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TB.Infrastructure;

using TB.Services.Interfaces;

namespace TB.Features.Downloads
{
    public class DownloadService : IDownloadService
    {
        private readonly List<DownloadItem> _downloads = new();
        private readonly string _dataFile;
        private int _nextId = 1;

        public event Action<DownloadItem>? OnProgress;
        public event Action<DownloadItem>? OnCompleted;
        public event Action<DownloadItem>? OnFailed;

        public IReadOnlyList<DownloadItem> Downloads => _downloads.AsReadOnly();

        public DownloadService(string basePath)
        {
            _dataFile = Path.Combine(basePath, "AppData", "downloads.json");
            Load();
        }

        public DownloadItem AddDownload(string url, string name, string filePath, long totalBytes)
        {
            var item = new DownloadItem
            {
                Id = _nextId++,
                Name = name,
                Url = url,
                FilePath = filePath,
                TotalBytes = totalBytes,
                Status = "downloading",
                Date = DateTime.Now
            };
            _downloads.Add(item);
            Save();
            return item;
        }

        public void UpdateProgress(int id, long receivedBytes)
        {
            var item = _downloads.FirstOrDefault(d => d.Id == id);
            if (item == null) return;
            item.ReceivedBytes = receivedBytes;
            Save();
            OnProgress?.Invoke(item);
        }

        public void CompleteDownload(int id)
        {
            var item = _downloads.FirstOrDefault(d => d.Id == id);
            if (item == null) return;
            item.Status = "completed";
            item.Date = DateTime.Now;
            Save();
            OnCompleted?.Invoke(item);
        }

        public void FailDownload(int id, string error)
        {
            var item = _downloads.FirstOrDefault(d => d.Id == id);
            if (item == null) return;
            item.Status = "failed";
            Save();
            OnFailed?.Invoke(item);
        }

        public void RemoveDownload(int id)
        {
            _downloads.RemoveAll(d => d.Id == id);
            Save();
        }

        public void ClearAll()
        {
            _downloads.Clear();
            Save();
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_downloads);
                File.WriteAllText(_dataFile, json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save downloads: {ex.Message}");
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_dataFile))
                {
                    var json = File.ReadAllText(_dataFile);
                    var list = JsonSerializer.Deserialize<List<DownloadItem>>(json);
                    if (list != null)
                    {
                        _downloads.Clear();
                        _downloads.AddRange(list);
                        _nextId = _downloads.Count > 0 ? _downloads.Max(d => d.Id) + 1 : 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load downloads: {ex.Message}");
            }
        }
    }
}
