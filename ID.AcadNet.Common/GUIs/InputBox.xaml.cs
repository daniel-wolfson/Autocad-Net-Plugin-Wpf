using System.Windows;

namespace Intellidesk.AcadNet.Common.GUIs
{
    /// <summary>
    /// InputBox.xaml code behind.
    /// </summary>
    public partial class InputBox : Window
    {
        public string Value => this.InputTextBox.Text;

        public InputBox()
        {
            InitializeComponent();
        }

        public InputBox(string defaultValue)
        {
            InitializeComponent();

            this.InputTextBox.Text = defaultValue;
        }

        public InputBox(string tip, string defaultValue)
        {
            InitializeComponent();

            this.Title = tip;
            this.InputTextBox.Text = defaultValue;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
