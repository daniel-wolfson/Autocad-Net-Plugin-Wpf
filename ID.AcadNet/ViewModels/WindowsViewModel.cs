using Intellidesk.AcadNet.Core;
using System;
using System.Windows.Input;

namespace Intellidesk.AcadNet.ViewModels
{
    public class WindowsViewModel : BaseViewModelWindow
    {
        private DateTime _textBox;
        public ICommand ClickCommand { get { return new BaseCommand(Click); } }

        private void Click()
        {
            TextBox = DateTime.Now;
        }

        public DateTime TextBox
        {
            get { return _textBox; }
            set
            {
                _textBox = value;
                OnPropertyChanged(() => TextBox);
            }
        }
    }
}