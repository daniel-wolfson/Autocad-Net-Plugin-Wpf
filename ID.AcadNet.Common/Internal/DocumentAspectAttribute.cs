using Autodesk.AutoCAD.ApplicationServices;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Internal
{
    [PSerializable]
    [LinesOfCodeAvoided(6)]
    public sealed class DocumentAspectAttribute : OnMethodBoundaryAspect
    {
        private Document doc = null;
        private bool IsSelectionEnabled { get; set; } = false;
        /// <summary>
        ///   Method invoked before the target method is executed.
        /// </summary>
        /// <param name="args">Method execution context.</param>
        public override void OnEntry(MethodExecutionArgs args)
        {
            doc = acadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                args.MethodExecutionTag = $"Error: Document not available"; ;
                args.FlowBehavior = FlowBehavior.Return;
                OnExit(args);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"Entering {doc.Name}");

            AppendCallInformation(args, stringBuilder);

            Plugin.Logger.Information($"{nameof(OnEntry)}: {stringBuilder}");
        }


        /// <summary>
        ///   Method invoked after the target method has successfully completed.
        /// </summary>
        /// <param name="args">Method execution context.</param>
        public override void OnSuccess(MethodExecutionArgs args)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Exiting ");

            AppendCallInformation(args, stringBuilder);

            if (!args.Method.IsConstructor && ((MethodInfo)args.Method).ReturnType != typeof(void))
            {
                stringBuilder.Append(" with return value ");
                stringBuilder.Append(args.ReturnValue);
            }

            Plugin.Logger.Information($"{doc?.Name ?? "document"}: {stringBuilder}");

            Notifications.SendNotifyMessageAsync(
                new NotifyArgs(NotifyStatus.Ready, "", $"command {doc?.Name ?? "document"} is ready"));
        }

        /// <summary>
        ///   Method invoked when the target method has failed.
        /// </summary>
        /// <param name="args">Method execution context.</param>
        public override void OnException(MethodExecutionArgs args)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("Exiting ");

            AppendCallInformation(args, stringBuilder);

            if (!args.Method.IsConstructor && ((MethodInfo)args.Method).ReturnType != typeof(void))
            {
                stringBuilder.Append(" with exception ");
                stringBuilder.Append(args.Exception.GetType().Name);
            }

            Plugin.Logger.Error($"{doc?.Name ?? "document"}: {stringBuilder}");

            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Error, "",
                    $"{doc?.Name ?? "document"} error: {args.Exception.InnerException?.Message ?? args.Exception.Message}"));
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Mouse.OverrideCursor = null;

            if (args.MethodExecutionTag is string errMessage && errMessage.Contains("Error"))
            {
                Notifications.SendNotifyMessageAsync(
                    new NotifyArgs(NotifyStatus.Error, "", $"{doc?.Name ?? ""} error: {errMessage}"));
            }
        }

        private static void AppendCallInformation(MethodExecutionArgs args, StringBuilder stringBuilder)
        {
            var declaringType = args.Method.DeclaringType;
            Formatter.AppendTypeName(stringBuilder, declaringType);
            stringBuilder.Append('.');
            stringBuilder.Append(args.Method.Name);

            if (args.Method.IsGenericMethod)
            {
                var genericArguments = args.Method.GetGenericArguments();
                Formatter.AppendGenericArguments(stringBuilder, genericArguments);
            }

            var arguments = args.Arguments;

            Formatter.AppendArguments(stringBuilder, arguments);
        }

        private void RegisterEvents()
        {
            UnregisterEvents();

            if (acadApp.DocumentManager.MdiActiveDocument != null)
            {
                acadApp.DocumentManager.DocumentActivated += OnDocumentActivated;
                acadApp.DocumentManager.DocumentToBeDestroyed += OnDocumentToBeDestroyed;
                acadApp.DocumentManager.MdiActiveDocument.CommandCancelled += OnCommandCancelled;

                //if (IsSelectionEnabled)
                //{
                //    acadApp.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged += OnImpliedSelectionChanged;
                //    acadApp.DocumentManager.MdiActiveDocument.Editor.SelectionAdded += OnSelectionAdded;
                //}
                //if (IsPointMonitorEnabled)
                //{
                //    acadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor += OnPointMonitor;
                //    // Need to enable the AutoCAD input event mechanism to do a pick under the prevailing
                //    // pick aperture on all digitizer events, regardless of whether a point is being acquired 
                //    // or whether any OSNAP modes are currently active.
                //    acadApp.DocumentManager.MdiActiveDocument.Editor.TurnForcedPickOn();
                //}
                //if (IsEntityModifyEnabled)
                //{
                //    Edit.ObjectErased += OnAcadEntityErased;
                //    Edit.ObjectModified += OnAcadEntityModified;
                //}
                //EventAggregator.GetEvent<NotifyMessageHandleEvent>().Subscribe(OnSelectionAddedEvent);
            }
        }

        private void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            UnregisterEvents();
        }

        private void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            RegisterEvents();
        }

        private void UnregisterEvents()
        {
            if (acadApp.DocumentManager.MdiActiveDocument != null)
            {
                acadApp.DocumentManager.DocumentActivated -= OnDocumentActivated;
                acadApp.DocumentManager.DocumentToBeDestroyed -= OnDocumentToBeDestroyed;
                acadApp.DocumentManager.MdiActiveDocument.CommandCancelled -= OnCommandCancelled;

                //if (IsSelectionEnabled)
                //{
                //    acadApp.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged -= OnImpliedSelectionChanged;
                //    acadApp.DocumentManager.MdiActiveDocument.Editor.SelectionAdded -= OnSelectionAdded;
                //}
                //if (IsPointMonitorEnabled)
                //{
                //    acadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor -= OnPointMonitor;
                //    acadApp.DocumentManager.MdiActiveDocument.Editor.TurnForcedPickOff();
                //}
                //if (IsEntityModifyEnabled)
                //{
                //    Edit.ObjectErased -= OnAcadEntityErased;
                //    Edit.ObjectModified -= OnAcadEntityModified;
                //}
                //EventAggregator.GetEvent<NotifyMessageHandleEvent>().Unsubscribe(OnSelectionAddedEvent);
            }
        }
    }
}
