using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Intellidesk.AcadNet.Common.Interfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Views
{
    /// <summary>
    /// Interaction logic for SplashScreenWindow.xaml
    /// </summary>
    public partial class WaitWindow : Window, IWaitWindow
    {
        public bool IsDisplayed = false;

        private readonly Dictionary<string, Dictionary<Func<bool>, string>> _tasks = 
            new Dictionary<string, Dictionary<Func<bool>, string>>();

        public void AddTask(string taskName, Func<bool> fn, string onSuccessMessage)
        {
            var taskDic = new Dictionary<Func<bool>, string> { { fn, onSuccessMessage } };
            _tasks.Add(taskName, taskDic);
        }

        public WaitWindow()
        {
            InitializeComponent();
        }

        public void StartAndWait()
        {
            foreach (var t in _tasks)
            {
                var fn = t.Value.Keys.FirstOrDefault();
                var t1 = t;

                using (var task = new Task<bool>(fn)) //Declare and  //initialize the task
                {
                    task.ContinueWith(result =>
                         {
                             AddMessage(t1.Value.ToString());
                             return true;
                         });
                    task.Wait();
                }
                //Task<bool>.Factory.StartNew(fn).ContinueWith(result =>
                //{
                //    AddMessage(t1.Value.ToString());
                //    return true;
                //});
            }

        }

        public void AddMessage(string message, int timeout = 0)
        {
            //while (true)
                //GIFCtrl.StartAnimate();
                Dispatcher.Invoke((Action)delegate()
                {
                    this.UpdateMessageTextBox.Text = message;
                });
                //Thread.Sleep(timeout);
        }

        public void AddMessage(string message, Action action)
        {
            Dispatcher.Invoke((Action)delegate()
            {
                this.UpdateMessageTextBox.Text = message;
                if (action != null)
                    action();
            });
        }

        public void Complete()
        {
            Dispatcher.InvokeShutdown();
        }

        public new void Show()
        {
            Application.ShowModelessWindow(Application.MainWindow.Handle, this, false);
            IsDisplayed = true;
        }
    }
}
