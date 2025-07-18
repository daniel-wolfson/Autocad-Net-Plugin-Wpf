using System;
using System.Windows;

namespace FileExplorer.Helper
{
    /// <summary>
    /// Image animation exception event
    /// </summary>
    public class ImageAnimExceptionRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Error exception class
        /// </summary>
        public Exception ErrorException;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="routedEvent">Routed event</param>
        /// <param name="obj">Object</param>
        public ImageAnimExceptionRoutedEventArgs(RoutedEvent routedEvent, object obj)
            : base(routedEvent, obj)
        {
        }
    }
}