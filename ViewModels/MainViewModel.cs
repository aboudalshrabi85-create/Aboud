using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AdvancedFileExplorer.Helpers;
using AdvancedFileExplorer.Models;
using AdvancedFileExplorer.Services;

namespace AdvancedFileExplorer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly FileService _fileService = new();
        private ExplorerTab? _selectedTab;
        private string _searchText = string.Empty;
        private string _viewMode = "Details";
        private bool _isPreviewPaneOpen = true;
        private FileItem? _selectedItem;
        private string _statusText = string.Empty;
        private string _selectionStatusText = string.Empty;
        private string _previewTitle = string.Empty;
        private string _previewType = string.Empty;
        private string _previewText = string.Empty;
        private string? _previewImagePath;
        private bool _hasImagePreview;
        private bool _hasTextPreview;
        private string _addressInputText = string.Empty;
        private bool _isEditingAddress;
        private string? _clipboardPath;
        private bool _isClipboardCut;

        public ObservableCollection<ExplorerTab> Tabs { get; } = new();
        public ObservableCollection<BreadcrumbItem> Breadcrumbs { get; } = new();
        public ObservableCollection<FileItem> Items { get; } = new();
        public ObservableCollection<FileItem> FilteredItems { get; } = new();
        public ObservableCollection<DriveItem> Drives { get; } = new();
        public ObservableCollection<QuickAccessItem> QuickAccessItems { get; } = new();

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(p => NavigateTo(p?.ToString()));
            BackCommand = new RelayCommand(_ => GoBack(), () => SelectedTab?.CanGoBack == true);
            ForwardCommand = new RelayCommand(_ => GoForward(), () => SelectedTab?.CanGoForward == true);
            UpCommand = new RelayCommand(_ => GoUp(), () => CanGoUp());
            RefreshCommand = new RelayCommand(_ => Refresh());
            NewTabCommand = new RelayCommand(_ => CreateNewTab());
            CloseTabCommand = new RelayCommand(p => CloseTab(p as ExplorerTab));
            OpenItemCommand = new RelayCommand(p => OpenItem(p as FileItem));
            ToggleViewModeCommand = new RelayCommand(p => ViewMode = p?.ToString() ?? "Details");
            TogglePreviewPaneCommand = new RelayCommand(_ => IsPreviewPaneOpen = !IsPreviewPaneOpen);
            NewFolderCommand = new RelayCommand(_ => PromptNewFolder());
            RenameCommand = new RelayCommand(_ => PromptRename(), () => SelectedItem != null);
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), () => SelectedItem != null);
            CopyCommand = new RelayCommand(_ => CopySelected(), () => SelectedItem != null);
            CutCommand = new RelayCommand(_ => CutSelected(), () => SelectedItem != null);
            PasteCommand = new RelayCommand(_ => PasteFromClipboard(), () => !string.IsNullOrEmpty(_clipboardPath));
            OpenInTerminalCommand = new RelayCommand(_ => OpenInTerminal());
            OpenPropertiesCommand = new RelayCommand(_ => ShowProperties(), () => SelectedItem != null);
            NavigateBreadcrumbCommand = new RelayCommand(p => NavigateTo(p?.ToString()));

            LoadDrivesAndQuickAccess();
            string initialPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!Directory.Exists(initialPath))
                initialPath = Drives.FirstOrDefault(d => d.IsReady)?.RootPath ?? @"C:\";
            CreateNewTab(initialPath);
        }

        public ExplorerTab? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    if (_selectedTab != null)
                    {
                        foreach (var t in Tabs) t.IsSelected = t == _selectedTab;
                        if (!string.IsNullOrEmpty(_selectedTab.CurrentPath))
                            LoadDirectory(_selectedTab.CurrentPath, false);
                    }
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
        }

        public string ViewMode { get => _viewMode; set => SetProperty(ref _viewMode, value); }
        public bool IsPreviewPaneOpen { get => _isPreviewPaneOpen; set => SetProperty(ref _isPreviewPaneOpen, value); }

        public FileItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    UpdatePreview();
                    UpdateStatus();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
        public string SelectionStatusText { get => _selectionStatusText; set => SetProperty(ref _selectionStatusText, value); }
        public string PreviewTitle { get => _previewTitle; set => SetProperty(ref _previewTitle, value); }
        public string PreviewType { get => _previewType; set => SetProperty(ref _previewType, value); }
        public string PreviewText { get => _previewText; set => SetProperty(ref _previewText, value); }
        public string? PreviewImagePath { get => _previewImagePath; set => SetProperty(ref _previewImagePath, value); }
        public bool HasImagePreview { get => _hasImagePreview; set => SetProperty(ref _hasImagePreview, value); }
        public bool HasTextPreview { get => _hasTextPreview; set => SetProperty(ref _hasTextPreview, value); }
        public string AddressInputText { get => _addressInputText; set => SetProperty(ref _addressInputText, value); }
        public bool IsEditingAddress { get => _isEditingAddress; set => SetProperty(ref _isEditingAddress, value); }

        public ICommand NavigateCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand UpCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NewTabCommand { get; }
        public ICommand CloseTabCommand { get; }
        public ICommand OpenItemCommand { get; }
        public ICommand ToggleViewModeCommand { get; }
        public ICommand TogglePreviewPaneCommand { get; }
        public ICommand NewFolderCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand OpenInTerminalCommand { get; }
        public ICommand OpenPropertiesCommand { get; }
        public ICommand NavigateBreadcrumbCommand { get; }

        public void LoadDrivesAndQuickAccess()
        {
            Drives.Clear();
            foreach (var d in _fileService.GetDrives()) Drives.Add(d);
            QuickAccessItems.Clear();
            foreach (var q in _fileService.GetQuickAccessItems()) QuickAccessItems.Add(q);
        }

        public void CreateNewTab(string? path = null)
        {
            string startPath = path ?? SelectedTab?.CurrentPath ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var tab = new ExplorerTab();
            Tabs.Add(tab);
            SelectedTab = tab;
            NavigateTo(startPath);
        }

        public void CloseTab(ExplorerTab? tab)
        {
            if (tab == null) return;
            int idx = Tabs.IndexOf(tab);
            bool wasSelected = SelectedTab == tab;
            Tabs.Remove(tab);
            if (Tabs.Count == 0) CreateNewTab();
            else if (wasSelected) SelectedTab = Tabs[Math.Max(0, idx - 1)];
        }

        public void NavigateTo(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                if (Directory.Exists(path))
                {
                    if (SelectedTab == null) CreateNewTab(path);
                    else { SelectedTab.NavigateTo(path); LoadDirectory(path, true); }
                }
                else if (File.Exists(path)) _fileService.OpenFile(path);
                else MessageBox.Show($"Cannot find directory:\n{path}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex) { MessageBox.Show($"Failed to navigate: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public void GoBack() { var path = SelectedTab?.GoBack(); if (path != null) LoadDirectory(path, false); }
        public void GoForward() { var path = SelectedTab?.GoForward(); if (path != null) LoadDirectory(path, false); }

        public bool CanGoUp()
        {
            if (string.IsNullOrEmpty(SelectedTab?.CurrentPath)) return false;
            try { return Directory.GetParent(SelectedTab.CurrentPath) != null; } catch { return false; }
        }

        public void GoUp()
        {
            if (string.IsNullOrEmpty(SelectedTab?.CurrentPath)) return;
            var parent = Directory.GetParent(SelectedTab.CurrentPath);
            if (parent != null) NavigateTo(parent.FullName);
        }

        public void Refresh()
        {
            if (!string.IsNullOrEmpty(SelectedTab?.CurrentPath))
            {
                LoadDirectory(SelectedTab.CurrentPath, false);
                LoadDrivesAndQuickAccess();
            }
        }

        private void LoadDirectory(string path, bool updateTabState)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
            AddressInputText = path;
            IsEditingAddress = false;
            SearchText = string.Empty;
            BuildBreadcrumbs(path);
            Items.Clear();
            foreach (var item in _fileService.GetDirectoryContents(path)) Items.Add(item);
            ApplyFilter();
            SelectedItem = null;
            UpdateStatus();
            CommandManager.InvalidateRequerySuggested();
        }

        private void BuildBreadcrumbs(string path)
        {
            Breadcrumbs.Clear();
            try
            {
                var curr = new DirectoryInfo(path);
                var segments = new List<BreadcrumbItem>();
                while (curr != null)
                {
                    string title = string.IsNullOrEmpty(curr.Parent?.FullName) && curr.FullName.EndsWith(@":\")
                        ? curr.FullName.TrimEnd('\\') : curr.Name;
                    segments.Insert(0, new BreadcrumbItem { Title = title, FullPath = curr.FullName });
                    curr = curr.Parent;
                }
                for (int i = 0; i < segments.Count; i++)
                {
                    segments[i].IsLast = i == segments.Count - 1;
                    Breadcrumbs.Add(segments[i]);
                }
            }
            catch { }
        }

        private void ApplyFilter()
        {
            FilteredItems.Clear();
            var query = SearchText?.Trim();
            foreach (var item in Items)
                if (string.IsNullOrEmpty(query) ||
                    item.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.Extension.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.FileType.Contains(query, StringComparison.OrdinalIgnoreCase))
                    FilteredItems.Add(item);
            UpdateStatus();
        }

        public void OpenItem(FileItem? item)
        {
            if (item == null) return;
            if (item.IsDirectory) NavigateTo(item.FullPath);
            else
            {
                try { _fileService.OpenFile(item.FullPath); }
                catch (Exception ex) { MessageBox.Show($"Could not open file: {ex.Message}", "Open Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        private void UpdatePreview()
        {
            if (SelectedItem == null)
            {
                PreviewTitle = PreviewType = PreviewText = string.Empty;
                PreviewImagePath = null;
                HasImagePreview = HasTextPreview = false;
                return;
            }

            PreviewTitle = SelectedItem.Name;
            PreviewType = SelectedItem.FileType;
            if (SelectedItem.IsDirectory)
            {
                HasImagePreview = HasTextPreview = false;
                PreviewText = $"Folder: {SelectedItem.Name}\nPath: {SelectedItem.FullPath}\nModified: {SelectedItem.FormattedDate}";
            }
            else if (_fileService.IsImage(SelectedItem.FullPath))
            {
                PreviewImagePath = SelectedItem.FullPath;
                HasImagePreview = true;
                HasTextPreview = false;
                PreviewText = string.Empty;
            }
            else
            {
                PreviewImagePath = null;
                HasImagePreview = false;
                HasTextPreview = true;
                PreviewText = _fileService.IsTextFile(SelectedItem.FullPath)
                    ? _fileService.GetFileTextPreview(SelectedItem.FullPath)
                    : $"File: {SelectedItem.Name}\nSize: {SelectedItem.FormattedSize}\nModified: {SelectedItem.FormattedDate}\nType: {SelectedItem.FileType}";
            }
        }

        public void UpdateStatus()
        {
            StatusText = $"{FilteredItems.Count} items";
            SelectionStatusText = SelectedItem == null ? string.Empty :
                SelectedItem.IsDirectory ? "1 item selected" : $"1 item selected ({SelectedItem.FormattedSize})";
        }

        public void PromptNewFolder()
        {
            if (string.IsNullOrEmpty(SelectedTab?.CurrentPath)) return;
            var dialog = new Views.InputDialog("New Folder", "Enter folder name:", "New Folder");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputResponse))
            {
                try { _fileService.CreateFolder(SelectedTab.CurrentPath, dialog.InputResponse.Trim()); Refresh(); }
                catch (Exception ex) { MessageBox.Show($"Failed to create folder:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        public void PromptRename()
        {
            if (SelectedItem == null) return;
            var dialog = new Views.InputDialog("Rename", "Enter new name:", SelectedItem.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputResponse))
            {
                try { _fileService.RenameItem(SelectedItem.FullPath, dialog.InputResponse.Trim()); Refresh(); }
                catch (Exception ex) { MessageBox.Show($"Failed to rename:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        public void DeleteSelected()
        {
            if (SelectedItem == null) return;
            if (MessageBox.Show($"Are you sure you want to delete '{SelectedItem.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try { _fileService.DeleteItem(SelectedItem.FullPath); Refresh(); }
                catch (Exception ex) { MessageBox.Show($"Failed to delete:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        public void CopySelected() { if (SelectedItem != null) { _clipboardPath = SelectedItem.FullPath; _isClipboardCut = false; } }
        public void CutSelected() { if (SelectedItem != null) { _clipboardPath = SelectedItem.FullPath; _isClipboardCut = true; SelectedItem.IsCut = true; } }

        public void PasteFromClipboard()
        {
            if (string.IsNullOrEmpty(_clipboardPath) || string.IsNullOrEmpty(SelectedTab?.CurrentPath)) return;
            try
            {
                if (_isClipboardCut) { _fileService.MoveItem(_clipboardPath, SelectedTab.CurrentPath); _clipboardPath = null; _isClipboardCut = false; }
                else _fileService.CopyItem(_clipboardPath, SelectedTab.CurrentPath);
                Refresh();
            }
            catch (Exception ex) { MessageBox.Show($"Paste failed:\n{ex.Message}", "Paste Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public void OpenInTerminal()
        {
            if (!string.IsNullOrEmpty(SelectedTab?.CurrentPath)) _fileService.OpenInTerminal(SelectedTab.CurrentPath);
        }

        public void ShowProperties()
        {
            if (SelectedItem == null) return;
            var win = new Views.PropertiesWindow(SelectedItem.FullPath) { Owner = Application.Current.MainWindow };
            win.ShowDialog();
        }
    }
}