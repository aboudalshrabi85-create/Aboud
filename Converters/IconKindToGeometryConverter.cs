using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdvancedFileExplorer.Converters
{
    public class IconKindToGeometryConverter : IValueConverter
    {
        private const string FolderPath = "M10 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z";
        private const string FilePath = "M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z";
        private const string ImagePath = "M21 19V5c0-1.1-.9-2-2-2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zM8.5 13.5l2.5 3.01L14.5 12l4.5 6H5l3.5-4.5z";
        private const string AudioPath = "M12 3v9.28c-.47-.17-.97-.28-1.5-.28C8.01 12 6 14.01 6 16.5S8.01 21 10.5 21c2.31 0 4.2-1.75 4.45-4H15V6h4V3h-7z";
        private const string VideoPath = "M17 10.5V7c0-.55-.45-1-1-1H4c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h12c.55 0 1-.45 1-1v-3.5l4 4v-11l-4 4z";
        private const string CodePath = "M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0l4.6-4.6-4.6-4.6L16 6l6 6-6 6-1.4-1.4z";
        private const string ArchivePath = "M20 6h-4V4c0-1.1-.9-2-2-2h-4c-1.1 0-2 .9-2 2v2H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2zM10 4h4v2h-4V4zm-1 5h6v2H9V9zm0 3h6v2H9v-2zm0 3h6v2H9v-2z";
        private const string ExecutablePath = "M19 4H5c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm-7 13l-4-4h3V9h2v4h3l-4 4z";
        private const string PdfPath = "M20 2H8c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm-8.5 7.5c0 .83-.67 1.5-1.5 1.5H9v2H7.5V7H10c.83 0 1.5.67 1.5 1.5v1zm5 2c0 .83-.67 1.5-1.5 1.5h-2.5V7H15c.83 0 1.5.67 1.5 1.5v3zm4-3H19v1h1.5V11H19v2h-1.5V7h3v1.5z";
        private const string DrivePath = "M20 18H4V6h16v12zm0-14H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm-4 12c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm2 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1z";
        private const string DesktopPath = "M21 2H3c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h7l-2 3v1h8v-1l-2-3h7c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 12H3V4h18v10z";
        private const string DownloadsPath = "M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z";
        private const string DocumentsPath = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string kind = value?.ToString() ?? "File";
            string pathString = kind switch
            {
                "Folder" => FolderPath,
                "Desktop" => DesktopPath,
                "Documents" => DocumentsPath,
                "Downloads" => DownloadsPath,
                "Pictures" or "Image" => ImagePath,
                "Music" or "Audio" => AudioPath,
                "Videos" or "Video" => VideoPath,
                "Code" => CodePath,
                "Archive" => ArchivePath,
                "Executable" => ExecutablePath,
                "Pdf" => PdfPath,
                "Drive" => DrivePath,
                _ => FilePath
            };
            return Geometry.Parse(pathString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IconKindToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string kind = value?.ToString() ?? "File";
            string hex = kind switch
            {
                "Folder" => "#FFC83B",
                "Desktop" => "#4CC2FF",
                "Documents" => "#0078D4",
                "Downloads" => "#26D07C",
                "Pictures" or "Image" => "#FF6E40",
                "Music" or "Audio" => "#E040FB",
                "Videos" or "Video" => "#FF5252",
                "Code" => "#40C4FF",
                "Archive" => "#FFD54F",
                "Executable" => "#00E676",
                "Pdf" => "#F44336",
                "Drive" => "#0078D4",
                _ => "#A0A0A0"
            };
            return (SolidColorBrush)new BrushConverter().ConvertFrom(hex)!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}