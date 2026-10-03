using System;
using System.Collections.Generic;
using System.IO;
using AdvancedFileExplorer.Helpers;

namespace AdvancedFileExplorer.Models
{
    public class ExplorerTab : ObservableObject
    {
        private string _title = "Home";
        private string _currentPath = string.Empty;
        private readonly List<string> _history = new();
        private int _historyIndex = -1;
        private bool _canGoBack;
        private bool _canGoForward;
        private bool _isSelected;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (SetProperty(ref _currentPath, value))
                    UpdateTitle();
            }
        }

        public bool CanGoBack
        {
            get => _canGoBack;
            private set => SetProperty(ref _canGoBack, value);
        }

        public bool CanGoForward
        {
            get => _canGoForward;
            private set => SetProperty(ref _canGoForward, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public void NavigateTo(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            path = Path.GetFullPath(path);

            if (_historyIndex >= 0 && _historyIndex < _history.Count &&
                string.Equals(_history[_historyIndex], path, StringComparison.OrdinalIgnoreCase))
                return;

            if (_historyIndex + 1 < _history.Count)
                _history.RemoveRange(_historyIndex + 1, _history.Count - (_historyIndex + 1));

            _history.Add(path);
            _historyIndex = _history.Count - 1;
            CurrentPath = path;
            UpdateNavigationState();
        }

        public string? GoBack()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                CurrentPath = _history[_historyIndex];
                UpdateNavigationState();
                return CurrentPath;
            }
            return null;
        }

        public string? GoForward()
        {
            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
                CurrentPath = _history[_historyIndex];
                UpdateNavigationState();
                return CurrentPath;
            }
            return null;
        }

        private void UpdateNavigationState()
        {
            CanGoBack = _historyIndex > 0;
            CanGoForward = _historyIndex < _history.Count - 1;
        }

        private void UpdateTitle()
        {
            if (string.IsNullOrEmpty(_currentPath))
            {
                Title = "This PC";
                return;
            }

            try
            {
                var dirInfo = new DirectoryInfo(_currentPath);
                Title = string.IsNullOrEmpty(dirInfo.Parent?.FullName) && dirInfo.FullName.EndsWith(@":\")
                    ? dirInfo.FullName.TrimEnd('\\')
                    : dirInfo.Name;
            }
            catch
            {
                Title = _currentPath;
            }
        }
    }
}