using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure.Enums;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> General </summary>
    public static class DispatcherExtensions
    {
        public static Editor Ed { get { return Doc.Editor; } }
        public static Database Db { get { return Doc.Database; } }
        public static Document Doc { get { return App.DocumentManager.MdiActiveDocument; } }


        /// <summary> Returns a value indicating whether the specified System.String[] object occurs within this string. </summary>
        public static bool XIsMatchFor(this string source, params string[] patterns)
        {
            return patterns.ToList().Any(x =>
            {
                x = x.Replace("*", ".*");
                //converting on start: "ABCD" to "^ABCD" and on end: "ABCD" to "ABCD$"
                x = Regex.Replace(x, @"^([^\*])", "^$1");
                x = Regex.Replace(x, @"([^\*])$", "$1$");

                //converting on start: "*ABCD" to ".*ABCD" and on end: "ABCD*" to "ABCD.*"
                //X = Regex.Replace(X, @"^\*", ".*");
                //X = Regex.Replace(X, @"\*$", ".*");
                return Regex.IsMatch(source, x);
            });
        }

        /// <summary> To make that changes appear on the screen </summary>
        public static void XTransFlush(this Transaction tr)
        {
            tr.TransactionManager.QueueForGraphicsFlush();
            App.DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        // Gets the column names collection of the datatable
        public static IEnumerable<string> GetColumnNames(this System.Data.DataTable dataTbl)
        {
            return dataTbl.Columns.Cast<System.Data.DataColumn>().Select(col => col.ColumnName);
        }

        public static void RunAsync_depricated(this Dispatcher dispatcher, object commandParameter,
            Action<object> callback, bool isRefreshAnimate = false)
        {
            Task.Run(() =>
            {
                if (isRefreshAnimate)
                {
                    var tupleParam = commandParameter as Tuple<object, object>;
                    if (tupleParam != null)
                    {
                        TextBlock tb = ((Tuple<object, object>)commandParameter).Item1 as TextBlock;
                        var rotateAnimation = new DoubleAnimation(0, 180, TimeSpan.FromSeconds(5));
                        var rt = (RotateTransform)tb.RenderTransform;
                        rt.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                    }
                }

                dispatcher.Invoke(() =>
                    callback(commandParameter));
            });
        }

        //public static AutoResetEvent resetEvent = new AutoResetEvent(true);

        public static void RunAsync<T>(this Dispatcher dispatcher, T commandParameter,
            Action<T> callback) where T : IPaletteElement
        {
            try
            {
                if (commandParameter is T)
                {
                    Task.Run(() =>
                    {
                        //resetEvent.WaitOne();
                        dispatcher.Invoke(() =>
                        {
                            callback(commandParameter);
                        });
                        //resetEvent.Set();
                    });
                }
            }
            catch (Exception)
            {
                //resetEvent.Reset();
            }
        }

        public static void NotifyUpdateAsync(this Dispatcher dispatcher, object commandParameter,
            Action<object> callback, bool isRefreshAnimate = false)
        {
            Task.Run(() =>
            {
                dispatcher.Invoke(() =>
                {
                    if (isRefreshAnimate)
                    {
                        var tupleParam = commandParameter as Tuple<object, object>;
                        if (tupleParam != null)
                        {
                            TextBlock tb = ((Tuple<object, object>)commandParameter).Item1 as TextBlock;
                            var rotateAnimation = new DoubleAnimation(0, 180, TimeSpan.FromSeconds(5));
                            var rt = (RotateTransform)tb.RenderTransform;
                            rt.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                        }
                    }

                    Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
                    Mouse.OverrideCursor = Cursors.Wait;

                    callback(commandParameter);

                    Mouse.OverrideCursor = null;
                    Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
                });
            });
        }

        public static void Update(this Dispatcher dispatcher, object commandParameter,
            Action<object> callback, bool isRefreshAnimate = false)
        {
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
            Mouse.OverrideCursor = Cursors.Wait;

            dispatcher.Invoke(() =>
            {
                if (isRefreshAnimate)
                {
                    var tupleParam = commandParameter as Tuple<object, object>;
                    if (tupleParam != null)
                    {
                        TextBlock tb = ((Tuple<object, object>)commandParameter).Item1 as TextBlock;
                        var rotateAnimation = new DoubleAnimation(0, 180, TimeSpan.FromSeconds(5));
                        var rt = (RotateTransform)tb.RenderTransform;
                        rt.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                    }
                }

                callback(commandParameter);
            });

            Mouse.OverrideCursor = null;
            Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
        }

        public static bool FindAny<T, T1>(this IEnumerable<T> list, T1 match) where T : IEqualityComparer<T>
        {
            var matchFound = false;
            Parallel.ForEach(list,
                (curValue, loopstate) =>
                {
                    if (curValue.Equals(match))
                    {
                        matchFound = true;
                        loopstate.Stop();
                    }
                });
            return matchFound;
        }
    }
}