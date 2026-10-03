// ========================================================================================
// اسم الملف: BreadcrumbItem.cs
// ========================================================================================

namespace AdvancedFileExplorer.Models
{
    public class BreadcrumbItem
    {
        public string Title { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsLast { get; set; }
    }
}