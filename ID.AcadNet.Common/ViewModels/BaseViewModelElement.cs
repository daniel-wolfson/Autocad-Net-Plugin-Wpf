using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.Data.Models.Entities;
using System;
using System.Windows.Threading;

namespace Intellidesk.AcadNet.Common.ViewModels
{
    public abstract class BaseViewModelElement : BaseEntity, IDisposable
    {
        protected static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
        public new IBasePanelContext Parent { get; set; }

        #region <UiDispatcher>

        public static Dispatcher UiDispatcher { get; set; }

        protected void RunOnUIThread(Action action)
        {
            if (null == action)
                return;

            if (UiDispatcher.CheckAccess())
                action();
            else
                UiDispatcher.Invoke(action, DispatcherPriority.Render);
        }

        protected async void RunOnUIThreadAsync(Action action)
        {
            if (null == action)
                return;

            if (UiDispatcher.CheckAccess())
                action();
            else
                await UiDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        #endregion <UiDispatcher>

        #region <IDisposable>

        protected bool isDisposed = false;
        protected virtual void OnDisposing(bool isDisposing)
        {
        }

        public void Dispose()
        {
            OnDisposing(true);
            GC.SuppressFinalize(this);
            isDisposed = true;
        }

        #endregion <IDisposable>
    }
}
