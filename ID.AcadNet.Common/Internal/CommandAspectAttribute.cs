using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Models;
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
    public sealed class CommandAspectAttribute : OnMethodBoundaryAspect
    {
        string _commandName;
        Document doc => acadApp.DocumentManager.MdiActiveDocument;
        public object CancelToken { get; set; } //CancellationToken
        public object CancelTokenSource { get; set; }

        public override bool CompileTimeValidate(MethodBase method)
        {
            object[] attributes = method.GetCustomAttributes(typeof(CommandMethodAttribute), true);
            if (attributes.Length > 0)
            {
                CommandMethodAttribute commandMethodAttribute = attributes[0] as CommandMethodAttribute;
                _commandName = commandMethodAttribute?.GlobalName ?? method.Name;
            }
            else
            {
                _commandName = method.Name;
            }

            var isCompile = base.CompileTimeValidate(method);

            return isCompile;
        }

        public CommandAspectAttribute()
        {
        }
        public CommandAspectAttribute(string command)
        {
            _commandName = command;
        }

        //[OnMethodEntryAdvice, MethodPointcut("SelectConstructors")]
        //public void OnConstructorEntry(MethodExecutionArgs args)
        //{
        //    _commandName = args.Arguments[0] as string;
        //}

        //IEnumerable<ConstructorInfo> SelectConstructors(EventInfo target)
        //{
        //    return target.DeclaringType.GetConstructors(
        //        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //}

        /// <summary> Method invoked before the target method is executed. </summary>
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!Utils.Utils.IsModelSpace())
            {
                args.MethodExecutionTag = $"Error: command {_commandName} available for model space only"; //(args.Method.Name);
                args.FlowBehavior = FlowBehavior.Return;
                OnExit(args);
                return;
            }

            //Mouse.OverrideCursor = Cursors.Wait;
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);

            CancelTokenSource = new CommandCancellationTokenSource();
            CancelToken = ((CommandCancellationTokenSource)CancelTokenSource).Token;

            if (!doc.UserData.ContainsKey(_commandName))
                doc.UserData.Add(_commandName, this);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Entering ");

            AppendCallInformation(args, stringBuilder);

            Plugin.Logger.Information($"{_commandName}: {stringBuilder}");
        }

        /// <summary> Method invoked after the target method has successfully completed. </summary>
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

            Plugin.Logger.Information($"{_commandName}: {stringBuilder}");

            Notifications.SendNotifyMessageAsync(
                new NotifyArgs(NotifyStatus.Ready, "", $"command {_commandName} is ready"));
        }

        /// <summary> Method invoked when the target method has failed. </summary>
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

            Plugin.Logger.Error($"{_commandName}: {stringBuilder}");

            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Error, "",
                    $"{_commandName} error: {args.Exception.InnerException?.Message ?? args.Exception.Message}"));
        }

        /// <summary>
        ///     Method executed after the body of methods to which this aspect is applied, even
        ///     when the method exists with an exception (this method is invoked from the finally
        ///     block).
        /// </summary>
        public override void OnExit(MethodExecutionArgs args)
        {
            Mouse.OverrideCursor = null;

            if (args.MethodExecutionTag is string errMessage && errMessage.Contains("Error"))
            {
                Notifications.SendNotifyMessageAsync(
                    new NotifyArgs(NotifyStatus.Error, "", $"{_commandName} error: {errMessage}"));
            }

            if (doc.UserData.ContainsKey(_commandName))
                doc.UserData.Remove(_commandName);
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
    }
}
