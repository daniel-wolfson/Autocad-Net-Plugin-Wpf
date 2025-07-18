using System;
using System.Threading;

namespace ID.Infrastructure
{
    public class CommandCanceledException : Exception
    {
        public CommandCanceledException(string message, Exception innerException)
            : base(message, innerException)
        {
            Text = message;
            CancellationToken = CancellationToken.None;
        }

        public CommandCanceledException(string message, Exception innerException, CancellationToken cancellationToken)
            : base(message, innerException)
        {
            Text = message;
            CancellationToken = cancellationToken;
        }

        public string Text { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }
}