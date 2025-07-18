using ID.Infrastructure.Models;

namespace ID.Infrastructure.Enums
{
    public enum NotifyStatus
    {
        [Notify("ErrorUndefined")] Undefined = 0,
        [Notify("Success")] Success = 1,
        [Notify("Ready")] Ready = 2,
        [Notify("Error")] Error = 4,
        [Notify("ErrorCancel")] Cancel = 8,
        [Notify("Ready")] Info = 16,
        [Notify("Loading")] Loading = 32,
        [Notify("Working")] Working = 64,
        [Notify("ErrorNotFound")] NotFound = 128
    }
}