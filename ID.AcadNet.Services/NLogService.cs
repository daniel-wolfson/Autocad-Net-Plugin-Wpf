using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System;
using System.Globalization;
using System.Text;
using System.Xml;
using Intellidesk.AcadNet.Common.Interfaces;
using Serilog;

// ReSharper disable once CheckNamespace
namespace Intellidesk.AcadNet.Services.Logging
{
    public class NLogService : INLogService
    {
        private static Logger _logger;
        private static string _loggerName;

        public static INLogService Create(string loggerName = "IDLogger")
        {
            _loggerName = loggerName;
            try
            {
                _logger = LogManager.GetLogger(loggerName, typeof(NLogService));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
            }
            
            return new NLogService();
        }

        public NLogService()
        {
            //var _logger = LogManager.GetCurrentClassLogger();
        }

        private LogEventInfo GetLogEvent(string loggerName, LogLevel level, Exception ex, string format = null, object[] args = null)
        {
            string assemblyProp = string.Empty;
            string classProp = string.Empty;
            string methodProp = string.Empty;
            string messageProp = string.Empty;
            string innerMessageProp = string.Empty;
            string stackTrace = string.Empty;

            var logEvent = new LogEventInfo
                (level, loggerName, string.Format(format ?? string.Empty, args ?? new object[] {}));

            if (ex != null)
            {
                assemblyProp = ex.Source;
                if (ex.TargetSite != null)
                {
                    classProp = ex.TargetSite.DeclaringType.FullName;
                    methodProp = ex.TargetSite.Name;
                }
                messageProp = ex.Message;

                if (ex.StackTrace != null)
                    stackTrace = ex.StackTrace;

                if (ex.InnerException != null)
                    innerMessageProp = ex.InnerException.Message;
            }

            logEvent.Properties["error-source"] = assemblyProp;
            logEvent.Properties["error-class"] = classProp;
            logEvent.Properties["error-method"] = methodProp;
            logEvent.Properties["error-message"] = messageProp;
            logEvent.Properties["inner-error-message"] = innerMessageProp;
            logEvent.Properties["stack-trace"] = stackTrace;

            return logEvent;
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }
        public void Info(Exception exception, string format, params object[] args)
        {
            var logEvent = GetLogEvent(_loggerName, LogLevel.Info, exception, string.Empty, null);
            _logger.Log(typeof(NLogService), logEvent);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        public void Debug(Exception ex, string format, params object[] args)
        {
            if (!_logger.IsDebugEnabled) return;
            var logEvent = GetLogEvent(_loggerName, LogLevel.Debug, ex, format, args);
            _logger.Log(typeof(NLogService), logEvent);
        }

        public void Error(string message)
        {
            _logger.Error(new Exception(message));
        }
        public void Error(string message, Exception ex)
        {
            //this.Error(new Exception(message));
            var logEvent = GetLogEvent(_loggerName, LogLevel.Error,  new Exception(message + "; " + ex.Message, ex.InnerException));
            _logger.Log(typeof(NLogService), logEvent);
        }
        public void Error(Exception ex, string format, params object[] args)
        {
            //if (!base.IsErrorEnabled) return;
            var logEvent = GetLogEvent(_loggerName, LogLevel.Error, ex, format, args);
            _logger.Log(typeof(NLogService), logEvent);
        }

        public void Warning(string message)
        {
            _logger.Warn(new Exception(message));
        }
        public void Warning(Exception exception, string format, params object[] args)
        {
            var logEvent = GetLogEvent(_loggerName, LogLevel.Warn, exception, format, args);
            _logger.Log(typeof(NLogService), logEvent);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(new Exception(message));
        }
        public void Fatal(Exception exception, string format, params object[] args)
        {
            if (!_logger.IsFatalEnabled) return;
            var logEvent = GetLogEvent(_loggerName, LogLevel.Fatal, exception, format, args);
            _logger.Log(typeof(NLogService), logEvent);
        }

        private string BuildExceptionMessage(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            Exception logException = ex;
            if (ex.InnerException != null)
                logException = ex.InnerException;

            //string strErrorMsg = Environment.NewLine + "Error in Path :" + System.Web.HttpContext.Current.Request.Path;
            //// Get the QueryString along with the Virtual Path
            //strErrorMsg += Environment.NewLine + "Raw Url :" + System.Web.HttpContext.Current.Request.RawUrl;

            // Get the error message
            sb.AppendLine("Message :" + logException.Message);

            // Source of the message
            sb.AppendLine("Source :" + logException.Source);

            // Stack Trace of the error
            sb.AppendLine("Stack Trace :" + logException.StackTrace);

            // Method where the error occurred
            sb.AppendLine("TargetSite :" + logException.TargetSite);

            return sb.ToString();
        }
    }

    [LayoutRenderer("web_variables")]
    public class WebVariablesRenderer : LayoutRenderer
    {
        public WebVariablesRenderer()
        {
            this.Format = "";
            this.Culture = CultureInfo.InvariantCulture;
        }

        /// Gets or sets the culture used for rendering.
        public CultureInfo Culture { get; set; }

        /// Gets or sets the date format. Can be any argument accepted by DateTime.ToString(format).
        [DefaultParameter]
        public string Format { get; set; }

        /// Renders the current date and appends it to the specified .
        /// <param name="builder">The  to append the rendered data to.
        /// <param name="logEvent">Logging event.
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);

            writer.WriteStartElement("error");

            // -----------------------------------------
            // Server Variables
            // -----------------------------------------
            writer.WriteStartElement("serverVariables");

            //foreach (string key in HttpContext.Current.Request.ServerVariables.AllKeys)
            //{
            //    writer.WriteStartElement("item");
            //    writer.WriteAttributeString("name", key);

            //    writer.WriteStartElement("value");
            //    writer.WriteAttributeString("string", HttpContext.Current.Request.ServerVariables[key].ToString());
            //    writer.WriteEndElement();

            //    writer.WriteEndElement();
            //}

            writer.WriteEndElement();

            // -----------------------------------------
            // Cookies
            // -----------------------------------------
            writer.WriteStartElement("cookies");

            //foreach (string key in HttpContext.Current.Request.Cookies.AllKeys)
            //{
            //    writer.WriteStartElement("item");
            //    writer.WriteAttributeString("name", key);

            //    writer.WriteStartElement("value");
            //    writer.WriteAttributeString("string", HttpContext.Current.Request.Cookies[key].Value.ToString());
            //    writer.WriteEndElement();

            //    writer.WriteEndElement();
            //}

            writer.WriteEndElement();
            // -----------------------------------------

            writer.WriteEndElement();
            // -----------------------------------------

            writer.Flush();
            writer.Close();

            string xml = sb.ToString();

            builder.Append(xml);
        }
    }

    [LayoutRenderer("utc_date")]
    public class UtcDateRenderer : LayoutRenderer
    {
        public UtcDateRenderer()
        {
            this.Format = "G";
            this.Culture = CultureInfo.InvariantCulture;
        }

        //protected override int GetEstimatedBufferSize(LogEventInfo ev)
        //{
        //    // Dates can be 6, 8, 10 bytes so let's go with 10
        //    return 10;
        //}

        /// Gets or sets the culture used for rendering.
        public CultureInfo Culture { get; set; }

        /// Gets or sets the date format. Can be any argument accepted by DateTime.ToString(format).
        [DefaultParameter]
        public string Format { get; set; }

        /// Renders the current date and appends it to the specified .
        /// <param name="builder">The  to append the rendered data to.
        /// <param name="logEvent">Logging event.
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(logEvent.TimeStamp.ToUniversalTime().ToString(this.Format, this.Culture));
        }
    }

}

// Register custom NLog Layout renderers
//LayoutRendererFactory.AddLayoutRenderer("utc_date", typeof(MySampleApp.Services.Logging.NLog.UtcDateRenderer));
//LayoutRendererFactory.AddLayoutRenderer("web_variables", typeof(MySampleApp.Services.Logging.NLog.WebVariablesRenderer));

