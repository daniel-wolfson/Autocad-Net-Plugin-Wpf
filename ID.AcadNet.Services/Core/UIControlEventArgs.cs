using System.Windows;

namespace Intellidesk.AcadNet.Services.Core
{
    public class UIControlEventArgs : RoutedEventArgs
    {
        public UIControlEventArgs(object sender, string commandArgs)
        {
            CurrentObject = sender;
            Command = commandArgs;
        }
        public object CurrentObject { get; set; }
        public string Command { get; set; }
    }
}