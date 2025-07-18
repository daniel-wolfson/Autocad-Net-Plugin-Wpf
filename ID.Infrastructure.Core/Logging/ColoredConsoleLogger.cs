using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace ID.Infrastructure.Logging
{
    public static class ColoredConsoleLoggerConfigurationExtensions
    {
        const string DefaultConsoleOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3} {Message}{NewLine}{Exception}";

        /// <summary>
        /// Now replaced by Serilog.Sinks.Console, please use that package instead. Writes log events 
        /// to <see cref="System.Console"/>, using color to differentiate between levels.
        /// </summary>
        /// <param name="sinkConfiguration">Logger sink configuration.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level
        /// to be changed at runtime.</param>
        /// <param name="outputTemplate">A message template describing the format used to write to the sink.
        /// the default is "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}".</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="standardErrorFromLevel">Specifies the level at which events will be written to standard error.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration ColoredConsole(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultConsoleOutputTemplate,
            IFormatProvider formatProvider = null,
            LoggingLevelSwitch levelSwitch = null,
            LogEventLevel? standardErrorFromLevel = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (outputTemplate == null) throw new ArgumentNullException(nameof(outputTemplate));
            return sinkConfiguration.Console(
                outputTemplate: outputTemplate,
                formatProvider: formatProvider,
                standardErrorFromLevel: standardErrorFromLevel,
                theme: ColoredConsoleTheme.Default,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                levelSwitch: levelSwitch);
        }
    }
}
