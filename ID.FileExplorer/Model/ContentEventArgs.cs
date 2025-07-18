using System;

namespace FileExplorer.Model
{
    public class ContentEventArgs<T> : EventArgs
    {
        public T Content { get; set; }
        public ContentEventArgs(T item)
        {
            this.Content = item;
        }
    }
}
