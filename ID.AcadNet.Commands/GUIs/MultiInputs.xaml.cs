using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Intellidesk.AcadNet.Commands.GUIs
{
    /// <summary>
    /// MultiInputs.xaml code behind.
    /// </summary>
    public partial class MultiInputs : Window
    {
        public Dictionary<string, string> Values { get; private set; }

        public MultiInputs()
        {
            InitializeComponent();
        }

        public void Ready(Dictionary<string, string> entries, string title = "Inputs")
        {
            this.Values = entries;
            foreach (var entry in entries)
            {
                var lable = new TextBlock
                {
                    Text = entry.Key,
                    ToolTip = entry.Key,
                    Width = 150,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var text = new TextBox
                {
                    Text = entry.Value
                };
                var row = new DockPanel
                {
                    Tag = entry
                };
                row.Children.Add(lable);
                row.Children.Add(text);
                DockPanel.SetDock(lable, Dock.Left);
                this.PropertyList.Children.Add(row);
            }
            this.Title = title;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DockPanel row in this.PropertyList.Children)
            {
                var entry = (KeyValuePair<string, string>)row.Tag;
                var text = row.Children[1] as TextBox;
                this.Values[entry.Key] = text.Text;
            }
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
