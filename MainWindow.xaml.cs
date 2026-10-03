using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdvancedFileExplorer.Models;
using AdvancedFileExplorer.ViewModels;

namespace AdvancedFileExplorer
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        public MainWindow() { InitializeComponent(); }
        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is ExplorerTab tab) ViewModel.SelectedTab = tab;
        }
        private void txtAddressInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { ViewModel.NavigateTo(txtAddressInput.Text.Trim()); e.Handled = true; }
            else if (e.Key == Key.Escape) { txtAddressInput.Text = ViewModel.AddressInputText; }
        }
        private void QuickAccess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem is QuickAccessItem item) { ViewModel.NavigateTo(item.FullPath); lb.SelectedItem = null; }
        }
        private void DriveItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem is DriveItem drive && drive.IsReady) ViewModel.NavigateTo(drive.RootPath);
        }
        private void FilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e) { ViewModel.OpenItem(ViewModel.SelectedItem); }
    }
}