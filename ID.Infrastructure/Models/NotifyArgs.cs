using ID.Infrastructure.Enums;

namespace ID.Infrastructure.Models
{
    public class ErrorNotifyArgs : NotifyArgs
    {
        public ErrorNotifyArgs(string tooltip = "")
            : base(NotifyStatus.Error, NotifyStatus.Error.ToString(""), tooltip)
        {
        }
    }

    public class CancelNotifyArgs : NotifyArgs
    {
        public CancelNotifyArgs(string tooltip = "")
            : base(NotifyStatus.Cancel, NotifyStatus.Cancel.ToString(""), tooltip)
        {
        }
    }

    public class InfoNotifyArgs : NotifyArgs
    {
        public InfoNotifyArgs(string tooltip = "")
            : base(NotifyStatus.Info, NotifyStatus.Info.ToString(), tooltip)
        {
        }
    }

    public class ReadyNotifyArgs : NotifyArgs
    {
        public ReadyNotifyArgs(string tooltip = "")
            : base(NotifyStatus.Ready, NotifyStatus.Ready.ToString(), tooltip)
        {
        }
    }

    public class NotifyArgs
    {
        public NotifyArgs(NotifyStatus status, string text = "", string tooltip = "", string command = "")
        {
            Status = status;
            Text = string.IsNullOrEmpty(text) ? status.ToString() : text;
            Text = status == NotifyStatus.Working || status == NotifyStatus.Loading ? Text + "..." : Text;
            Tooltip = string.IsNullOrEmpty(tooltip) ? Text : tooltip;
            CommanName = command;
        }
        public NotifyStatus Status { get; set; }
        public string Text { get; set; }
        public string Tooltip { get; set; }

        public string CommanName { get; set; }
    }
}