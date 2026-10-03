// ========================================================================================
// اسم الملف: FileItem.cs
// المجلد: Models (نماذج البيانات)
// وصف الملف: يمثل نموذج بيانات الملف أو المجلد (File or Folder Entity)
// مستوى الطالب: تقنية معلومات - مستوى ثالث (مادة برمجة مرئية / C# و نظم تشغيل)
// ========================================================================================

using System;
using AdvancedFileExplorer.Helpers;

namespace AdvancedFileExplorer.Models
{
    public class FileItem : ObservableObject
    {
        private bool _isSelected;
        private bool _isCut;

        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public string Extension { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public DateTime DateModified { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string IconKind { get; set; } = "File";

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsCut
        {
            get => _isCut;
            set => SetProperty(ref _isCut, value);
        }
    }
}