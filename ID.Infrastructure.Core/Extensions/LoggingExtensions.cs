using Serilog;
using Serilog.Context;
using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace ID.Infrastructure.Extensions
{
    public static class LoggingExtensions
    {
        public static void WarningEx(this ILogger logger, Exception exception)
        {
            logger.Warning(exception, (exception.InnerException ?? exception).Message);
        }
        public static void ErrorEx(this ILogger logger, Exception exception)
        {
            logger.Error(exception, (exception.InnerException ?? exception).Message);
        }

        public static void ErrorCall(this ILogger logger, Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            using (var propMemberName = LogContext.PushProperty("MemberName", memberName))
            using (var propFilePath = LogContext.PushProperty("FilePath", sourceFilePath))
            using (var propLineNumber = LogContext.PushProperty("LineNumber", sourceLineNumber))
            {
                logger.Error(exception, (exception.InnerException ?? exception).Message);
            }
        }

        public static void ErrorCall(this ILogger logger, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            using (var propMemberName = LogContext.PushProperty("MemberName", memberName))
            using (var propFilePath = LogContext.PushProperty("FilePath", sourceFilePath))
            using (var propLineNumber = LogContext.PushProperty("LineNumber", sourceLineNumber))
            {
                logger.Error(message);
            }
        }

        public static void InfoCall(this ILogger logger, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            using (var propMemberName = LogContext.PushProperty("MemberName", memberName))
            using (var propFilePath = LogContext.PushProperty("FilePath", sourceFilePath))
            using (var propLineNumber = LogContext.PushProperty("LineNumber", sourceLineNumber))
            {
                logger.Information(message);
            }
        }

        public static void WarnCall(this ILogger logger, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            using (var propMemberName = LogContext.PushProperty("MemberName", memberName))
            using (var propFilePath = LogContext.PushProperty("FilePath", sourceFilePath))
            using (var propLineNumber = LogContext.PushProperty("LineNumber", sourceLineNumber))
            {
                logger.Warning(message);
            }
        }

        public static void LogCall(this ILogger logger, EventLevel eventLevel, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            using (var propMemberName = LogContext.PushProperty("MemberName", memberName))
            using (var propFilePath = LogContext.PushProperty("FilePath", sourceFilePath))
            using (var propLineNumber = LogContext.PushProperty("LineNumber", sourceLineNumber))
            {
                switch (eventLevel)
                {
                    case EventLevel.Critical:
                        logger.Error(message);
                        break;

                    case EventLevel.Error:
                        logger.Error(message);
                        break;

                    case EventLevel.Warning:
                        logger.Warning(message);
                        break;

                    case EventLevel.LogAlways:
                    case EventLevel.Verbose:
                    case EventLevel.Informational:
                    default:
                        logger.Information(message);
                        break;
                }
            }
        }
    }
}
