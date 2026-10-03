using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AdvancedFileExplorer.Models;

namespace AdvancedFileExplorer.Services
{
    public class FileService
    {
        public List<DriveItem> GetDrives()
        {
            var drives = new List<DriveItem>();
            try
            {
                foreach (var d in DriveInfo.GetDrives())
                {
                    var item = new DriveItem
                    {
                        RootPath = d.RootDirectory.FullName,
                        DriveType = d.DriveType.ToString(),
                        IsReady = d.IsReady
                    };
                    if (d.IsReady)
                    {
                        item.VolumeLabel = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "Local Disk" : d.VolumeLabel;
                        item.Name = $"{item.VolumeLabel} ({d.Name.TrimEnd('\\')})";
                        item.TotalBytes = d.TotalSize;
                        item.FreeBytes = d.TotalFreeSpace;
                        item.UsedBytes = d.TotalSize - d.TotalFreeSpace;
                        item.UsedPercentage = d.TotalSize > 0 ? ((double)item.UsedBytes / d.TotalSize) * 100 : 0;
                        item.TotalSpaceFormatted = FormatBytes(d.TotalSize);
                        item.FreeSpaceFormatted = FormatBytes(d.TotalFreeSpace);
                    }
                    else
                    {
                        item.Name = $"Drive ({d.Name.TrimEnd('\\')})";
                        item.VolumeLabel = "Removable / Not Ready";
                    }
                    drives.Add(item);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return drives;
        }

        public List<QuickAccessItem> GetQuickAccessItems()
        {
            var items = new List<QuickAccessItem>();
            void AddIfValid(string name, Environment.SpecialFolder folder, string icon)
            {
                try
                {
                    var path = Environment.GetFolderPath(folder);
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        items.Add(new QuickAccessItem { Name = name, FullPath = path, IconKind = icon });
                }
                catch { }
            }
            AddIfValid("Desktop", Environment.SpecialFolder.Desktop, "Desktop");
            AddIfValid("Documents", Environment.SpecialFolder.MyDocuments, "Documents");
            try
            {
                string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string downloads = Path.Combine(userPath, "Downloads");
                if (Directory.Exists(downloads))
                    items.Add(new QuickAccessItem { Name = "Downloads", FullPath = downloads, IconKind = "Downloads" });
            }
            catch { }
            AddIfValid("Pictures", Environment.SpecialFolder.MyPictures, "Pictures");
            AddIfValid("Music", Environment.SpecialFolder.MyMusic, "Music");
            AddIfValid("Videos", Environment.SpecialFolder.MyVideos, "Videos");
            return items;
        }

        public List<FileItem> GetDirectoryContents(string path)
        {
            var result = new List<FileItem>();
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return result;
            try
            {
                var dirInfo = new DirectoryInfo(path);
                try
                {
                    foreach (var dir in dirInfo.EnumerateDirectories())
                    {
                        if ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                        result.Add(new FileItem
                        {
                            Name = dir.Name, FullPath = dir.FullName, IsDirectory = true,
                            Extension = string.Empty, SizeBytes = 0, FormattedSize = string.Empty,
                            DateModified = dir.LastWriteTime, FormattedDate = dir.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                            FileType = "File folder", IconKind = "Folder"
                        });
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }

                try
                {
                    foreach (var file in dirInfo.EnumerateFiles())
                    {
                        if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                        string ext = file.Extension.ToLowerInvariant();
                        result.Add(new FileItem
                        {
                            Name = file.Name, FullPath = file.FullName, IsDirectory = false,
                            Extension = ext, SizeBytes = file.Length, FormattedSize = FormatBytes(file.Length),
                            DateModified = file.LastWriteTime, FormattedDate = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                            FileType = GetFileTypeDescription(ext), IconKind = GetIconKindForExtension(ext)
                        });
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return result;
        }

        public void OpenFile(string path)
        {
            try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
            catch (Exception ex) { throw new InvalidOperationException($"Could not open file: {ex.Message}", ex); }
        }

        public void OpenInTerminal(string folderPath)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "powershell.exe", WorkingDirectory = folderPath, UseShellExecute = true }); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public void CreateFolder(string parentPath, string folderName)
        {
            string targetPath = Path.Combine(parentPath, folderName);
            if (Directory.Exists(targetPath)) throw new InvalidOperationException("A folder with this name already exists.");
            Directory.CreateDirectory(targetPath);
        }

        public void DeleteItem(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
            else if (File.Exists(path)) File.Delete(path);
        }

        public void RenameItem(string oldPath, string newName)
        {
            string? parent = Path.GetDirectoryName(oldPath);
            if (parent == null) throw new InvalidOperationException("Invalid path.");
            string newPath = Path.Combine(parent, newName);
            if (Directory.Exists(oldPath)) Directory.Move(oldPath, newPath);
            else if (File.Exists(oldPath)) File.Move(oldPath, newPath);
        }

        public void CopyItem(string sourcePath, string targetDirectory)
        {
            if (File.Exists(sourcePath))
            {
                string destFile = GetUniqueFilePath(Path.Combine(targetDirectory, Path.GetFileName(sourcePath)));
                File.Copy(sourcePath, destFile);
            }
            else if (Directory.Exists(sourcePath))
            {
                string dirName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                string destDir = GetUniqueDirectoryPath(Path.Combine(targetDirectory, dirName));
                CopyDirectoryRecursive(sourcePath, destDir);
            }
        }

        public void MoveItem(string sourcePath, string targetDirectory)
        {
            if (File.Exists(sourcePath))
                File.Move(sourcePath, GetUniqueFilePath(Path.Combine(targetDirectory, Path.GetFileName(sourcePath))));
            else if (Directory.Exists(sourcePath))
            {
                string dirName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                Directory.Move(sourcePath, GetUniqueDirectoryPath(Path.Combine(targetDirectory, dirName)));
            }
        }

        private static void CopyDirectoryRecursive(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            foreach (var subDir in Directory.GetDirectories(sourceDir))
                CopyDirectoryRecursive(subDir, Path.Combine(targetDir, Path.GetFileName(subDir)));
        }

        private static string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath)) return filePath;
            string dir = Path.GetDirectoryName(filePath) ?? "";
            string name = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);
            int count = 1;
            string path;
            do { path = Path.Combine(dir, $"{name} ({count}){ext}"); count++; } while (File.Exists(path));
            return path;
        }

        private static string GetUniqueDirectoryPath(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return dirPath;
            string parent = Path.GetDirectoryName(dirPath) ?? "";
            string name = Path.GetFileName(dirPath);
            int count = 1;
            string path;
            do { path = Path.Combine(parent, $"{name} ({count})"); count++; } while (Directory.Exists(path));
            return path;
        }

        public string GetFileTextPreview(string path, int maxChars = 3000)
        {
            try
            {
                if (!File.Exists(path)) return string.Empty;
                using var reader = new StreamReader(path);
                char[] buffer = new char[maxChars];
                int read = reader.ReadBlock(buffer, 0, maxChars);
                string text = new string(buffer, 0, read);
                if (reader.Peek() >= 0) text += "\n\n... [Content truncated for preview] ...";
                return text;
            }
            catch (Exception ex) { return $"[Cannot preview file: {ex.Message}]"; }
        }

        public bool IsImage(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".webp" or ".ico";
        }

        public bool IsTextFile(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".txt" or ".cs" or ".xaml" or ".xml" or ".json" or ".md" or ".html" or ".css" or ".js" or ".ts" or ".py" or ".cpp" or ".h" or ".c" or ".java" or ".ini" or ".cfg" or ".log" or ".sql" or ".bat" or ".ps1" or ".sh";
        }

        public static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            if (bytes == 0) return "0 B";
            int mag = (int)Math.Max(0, Math.Floor(Math.Log(bytes, 1024)));
            if (mag >= suffixes.Length) mag = suffixes.Length - 1;
            double adjusted = bytes / Math.Pow(1024, mag);
            return $"{adjusted:0.##} {suffixes[mag]}";
        }

        public static string GetFileTypeDescription(string ext)
        {
            return ext switch
            {
                ".txt" => "Text Document", ".cs" => "C# Source File", ".xaml" => "XAML Document",
                ".json" => "JSON File", ".xml" => "XML Document", ".md" => "Markdown File",
                ".pdf" => "PDF Document", ".png" => "PNG Image", ".jpg" or ".jpeg" => "JPEG Image",
                ".gif" => "GIF Image", ".bmp" => "Bitmap Image", ".webp" => "WebP Image", ".svg" => "SVG Vector Image",
                ".mp3" or ".wav" or ".flac" or ".aac" => "Audio File", ".mp4" or ".mkv" or ".avi" or ".mov" => "Video File",
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "Compressed Archive", ".exe" => "Application",
                ".msi" => "Windows Installer Package", ".dll" => "Application Extension (DLL)",
                ".doc" or ".docx" => "Microsoft Word Document", ".xls" or ".xlsx" => "Microsoft Excel Worksheet",
                ".ppt" or ".pptx" => "Microsoft PowerPoint Presentation",
                _ => string.IsNullOrEmpty(ext) ? "File" : $"{ext.TrimStart('.').ToUpper()} File"
            };
        }

        public static string GetIconKindForExtension(string ext)
        {
            return ext switch
            {
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" or ".svg" or ".ico" => "Image",
                ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" or ".m4a" => "Audio",
                ".mp4" or ".mkv" or ".avi" or ".mov" or ".wmv" or ".flv" => "Video",
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".iso" => "Archive",
                ".exe" or ".msi" or ".bat" or ".cmd" or ".ps1" => "Executable",
                ".pdf" => "Pdf",
                ".cs" or ".xaml" or ".xml" or ".json" or ".html" or ".css" or ".js" or ".ts" or ".py" or ".cpp" or ".c" or ".h" or ".java" or ".sql" => "Code",
                ".txt" or ".md" or ".doc" or ".docx" or ".log" or ".ini" or ".cfg" => "Text",
                _ => "File"
            };
        }
    }
}