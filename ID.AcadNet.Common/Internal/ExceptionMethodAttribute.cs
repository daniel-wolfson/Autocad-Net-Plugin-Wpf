using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Text;

namespace Intellidesk.AcadNet.Common.Internal
{
    [PSerializable]
    public class ExceptionMethodAttribute : OnExceptionAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            Console.WriteLine(args.Exception.Message);
            args.FlowBehavior = FlowBehavior.Return;
            args.ReturnValue = -1;
        }
    }

    internal static class Formatter
    {
        public static void AppendTypeName(StringBuilder stringBuilder, Type declaringType)
        {
            stringBuilder.Append(declaringType.FullName);
            if (declaringType.IsGenericType)
            {
                var genericArguments = declaringType.GetGenericArguments();
                AppendGenericArguments(stringBuilder, genericArguments);
            }
        }

        public static void AppendGenericArguments(StringBuilder stringBuilder, Type[] genericArguments)
        {
            stringBuilder.Append('<');
            for (var i = 0; i < genericArguments.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(genericArguments[i].Name);
            }
            stringBuilder.Append('>');
        }

        public static void AppendArguments(StringBuilder stringBuilder, Arguments arguments)
        {
            stringBuilder.Append('(');
            for (var i = 0; i < arguments.Count; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(arguments[i]);
            }
            stringBuilder.Append(')');
        }
    }
}
