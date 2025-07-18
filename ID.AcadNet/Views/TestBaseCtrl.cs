using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Views
{
    public sealed partial class TestBaseCtrl : UserControl
    {
        public static RoutedCommand TestButton1 = new RoutedCommand();
        public static RoutedCommand TestButton2 = new RoutedCommand();

        static TestBaseCtrl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TestBaseCtrl), new FrameworkPropertyMetadata(typeof(TestBaseCtrl)));
        }

        public TestBaseCtrl()
        {
            this.CommandBindings.Add(new System.Windows.Input.CommandBinding(TestButton1, TestButton1_Executed));
            this.CommandBindings.Add(new System.Windows.Input.CommandBinding(TestButton2, TestButton2_Executed));
        }

        private void TestButton1_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("This is button 1 from the base control");
        }

        private void TestButton2_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("This is button 2 from the base control");
        }
    }
}