using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Intellidesk.AcadNet.Views
{
    /// <summary>
    /// Interaction logic for MyWPFUserControl.xaml
    /// </summary>
    public partial class MyWPFUserControl : UserControl
    {
        
        private Dispatcher UiDispatcher { get; set; }
        private Dispatcher BackgroundDispatcher { get; set; }
        private readonly object SyncObj = new object();
        private bool stop;
        public MyWPFUserControl()
        {
            InitializeComponent();
            UiDispatcher = Dispatcher.CurrentDispatcher;
            new Thread(CreateBackGroundThread).Start();
        }


        private bool Stop
        {
            get
            {
                lock (SyncObj) { return stop; }
            }
            set
            {
                lock (SyncObj) { stop = value; }
            }
        }

        private void CreateBackGroundThread() {
            BackgroundDispatcher = Dispatcher.CurrentDispatcher;
            Dispatcher.Run();
        }

        private void Start() {
            while (UiDispatcher.Invoke(() => prg.Value < prg.Maximum))
            {
                if (Stop) return;
                UiDispatcher.Invoke(new Action(() => prg.Value++));
                Thread.Sleep(3000);
            }
        }

        private void Start_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            prg.Value = prg.Minimum;
            Stop = false;
            BackgroundDispatcher.BeginInvoke(new Action(Start));
        }

        private void Stop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Stop = true;
        }
    }
}
