using Prism.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ID.Infrastructure
{
    public class InfraManager
    {
        public static Dispatcher UiDispatcher { get; set; }
        public static Dispatcher BackgroundDispatcher { get; set; }

        private static IEventAggregator _eventAggregator;

        static InfraManager()
        {
            _eventAggregator = Plugin.GetService<IEventAggregator>();

            UiDispatcher = Dispatcher.CurrentDispatcher;
            new Thread(() =>
            {
                BackgroundDispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            }).Start();
        }
        public static void RunOnUIThread(Action action)
        {
            if (null == action) return;

            if (UiDispatcher.CheckAccess())
                action();
            else
                UiDispatcher.Invoke(action);
        }

        /// <summary> RunOnUIThreadAsync </summary>
        public static async Task RunOnUIThreadAsync(Action action)
        {
            if (null == action) return;
            await UiDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        public static void DelayAction(int millisecond, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                action.Invoke();
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }
    }
}
