using System.Windows;
using System.Windows.Input;

namespace AdvancedFileExplorer.Views
{
    public partial class InputDialog : Window
    {
        public string InputResponse => txtInput.Text;

        public InputDialog(string title, string prompt, string defaultText = "")
        {
            InitializeComponent();
            Title = title;
            lblPrompt.Text = prompt;
            txtInput.Text = defaultText;
            Loaded += (s, e) =>
            {
                txtInput.Focus();
                txtInput.SelectAll();
            };
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}