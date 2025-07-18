using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ID.Infrastructure.Logging
{
    static class ColoredConsoleTheme
    {
        public static SystemConsoleTheme Default { get; } = new SystemConsoleTheme(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.TertiaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.Invalid] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.Null] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray, Background = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Blue },
                [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray, Background = ConsoleColor.Yellow },
                [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
                [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red }
            });
    }



    /// <summary>
    /// A console theme using the ANSI terminal escape sequences. Recommended
    /// for Linux and Windows 10+.
    /// </summary>
    public class AnsiConsoleTheme : ConsoleTheme
    {
        /// <summary>
        /// A 256-color theme along the lines of Visual Studio Code.
        /// </summary>
        public static AnsiConsoleTheme Code { get; } = AnsiConsoleTheme.Code;

        /// <summary>
        /// A theme using only gray, black and white.
        /// </summary>
        public static AnsiConsoleTheme Grayscale { get; } = AnsiConsoleTheme.Grayscale;

        /// <summary>
        /// A theme in the style of the original <i>Serilog.Sinks.Literate</i>.
        /// </summary>
        public static AnsiConsoleTheme Literate { get; } = AnsiConsoleTheme.Literate;

        readonly IReadOnlyDictionary<ConsoleThemeStyle, string> _styles;
        const string AnsiStyleReset = "\x1b[0m";

        /// <summary>
        /// Construct a theme given a set of styles.
        /// </summary>
        /// <param name="styles">Styles to apply within the theme.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="styles"/> is <code>null</code></exception>
        public AnsiConsoleTheme(IReadOnlyDictionary<ConsoleThemeStyle, string> styles)
        {
            if (styles is null) throw new ArgumentNullException(nameof(styles));
            _styles = styles.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <inheritdoc/>
        public override bool CanBuffer => true;

        /// <inheritdoc/>
        protected override int ResetCharCount { get; } = AnsiStyleReset.Length;

        /// <inheritdoc/>
        public override int Set(TextWriter output, ConsoleThemeStyle style)
        {
            if (_styles.TryGetValue(style, out var ansiStyle))
            {
                output.Write(ansiStyle);
                return ansiStyle.Length;
            }
            return 0;
        }

        /// <inheritdoc/>
        public override void Reset(TextWriter output)
        {
            output.Write(AnsiStyleReset);
        }
    }
}
