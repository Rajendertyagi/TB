using System;

namespace TB.Features.Downloads
{
    public class DownloadItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long TotalBytes { get; set; }
        public long ReceivedBytes { get; set; }
        public double ProgressPercent => TotalBytes > 0 ? Math.Round((double)ReceivedBytes / TotalBytes * 100, 1) : 0;
        public string Status { get; set; } = "downloading";
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
