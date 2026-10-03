using AdvancedFileExplorer.Helpers;

namespace AdvancedFileExplorer.Models
{
    public class DriveItem : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string VolumeLabel { get; set; } = string.Empty;
        public string RootPath { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
        public long UsedBytes { get; set; }
        public double UsedPercentage { get; set; }
        public string TotalSpaceFormatted { get; set; } = string.Empty;
        public string FreeSpaceFormatted { get; set; } = string.Empty;
        public string DriveType { get; set; } = "Fixed";
        public bool IsReady { get; set; } = true;
    }
}