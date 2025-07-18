using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace ID.Infrastructure.Models
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private readonly ReaderWriterLockSlim _itemsLock = new ReaderWriterLockSlim();
        private SynchronizationContext _currentContext;

        public ObservableRangeCollection()
        {
            _currentContext = SynchronizationContext.Current;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _currentContext.Send(state => base.OnCollectionChanged(e), null);
        }

        public void AddRange(IEnumerable<T> list, Action contextAction = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            foreach (T item in list)
            {
                _itemsLock.EnterWriteLock();
                try
                {
                    Add(item);
                }
                finally
                {
                    _itemsLock.ExitWriteLock();
                }
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            if (contextAction != null)
                _currentContext.Send((c) => contextAction(), null);
        }

        public new void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}