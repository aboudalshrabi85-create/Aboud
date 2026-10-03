using System;
using System.IO;
using System.Windows;
using AdvancedFileExplorer.Services;

namespace AdvancedFileExplorer.Views
{
    public partial class PropertiesWindow : Window
    {
        private readonly string _targetPath;
        private readonly bool _isDir;

        public PropertiesWindow(string path)
        {
            InitializeComponent();
            _targetPath = path;
            _isDir = Directory.Exists(path);
            LoadItemInfo();
        }

        private void LoadItemInfo()
        {
            try
            {
                if (_isDir)
                {
                    var dir = new DirectoryInfo(_targetPath);
                    Title = $"{dir.Name} Properties";
                    txtName.Text = dir.Name;
                    lblType.Text = "File folder";
                    lblLocation.Text = dir.Parent?.FullName ?? dir.FullName;
                    long totalSize = 0; int fileCount = 0; int dirCount = 0;
                    try
                    {
                        foreach (var f in dir.EnumerateFiles("*", SearchOption.AllDirectories)) { totalSize += f.Length; fileCount++; }
                        foreach (var d in dir.EnumerateDirectories("*", SearchOption.AllDirectories)) dirCount++;
                        lblSize.Text = $"{FileService.FormatBytes(totalSize)} ({totalSize:N0} bytes) — {fileCount} Files, {dirCount} Folders";
                    }
                    catch { lblSize.Text = "Unknown (Access restricted)"; }
                    lblCreated.Text = dir.CreationTime.ToString("F");
                    lblModified.Text = dir.LastWriteTime.ToString("F");
                    lblAccessed.Text = dir.LastAccessTime.ToString("F");
                    chkReadOnly.IsChecked = (dir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                    chkHidden.IsChecked = (dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                }
                else if (File.Exists(_targetPath))
                {
                    var file = new FileInfo(_targetPath);
                    Title = $"{file.Name} Properties";
                    txtName.Text = file.Name;
                    lblType.Text = FileService.GetFileTypeDescription(file.Extension.ToLowerInvariant());
                    lblLocation.Text = file.DirectoryName ?? file.FullName;
                    lblSize.Text = $"{FileService.FormatBytes(file.Length)} ({file.Length:N0} bytes)";
                    lblCreated.Text = file.CreationTime.ToString("F");
                    lblModified.Text = file.LastWriteTime.ToString("F");
                    lblAccessed.Text = file.LastAccessTime.ToString("F");
                    chkReadOnly.IsChecked = (file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                    chkHidden.IsChecked = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                }
            }
            catch (Exception ex) { MessageBox.Show($"Could not read properties: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var attr = File.GetAttributes(_targetPath);
                if (chkReadOnly.IsChecked == true) attr |= FileAttributes.ReadOnly; else attr &= ~FileAttributes.ReadOnly;
                if (chkHidden.IsChecked == true) attr |= FileAttributes.Hidden; else attr &= ~FileAttributes.Hidden;
                File.SetAttributes(_targetPath, attr);
            }
            catch { }
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}