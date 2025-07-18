using System;

namespace ID.SignalRSelfHost.Lib
{
    public class AcadCommandTrack
    {
        public string UserName { set; get; }
        public string ComputerName { set; get; }
        public string CommandName { set; get; }
        public DateTime CmdExecTime { set; get; }
        public string DwgFileName { set; get; }

        public override string ToString()
        {
            return "Command \"" + CommandName + "\"" +
                " executed in drawing \"" + DwgFileName + "\"" +
                " at " + CmdExecTime.ToLongTimeString() +
                " on computer \"" + ComputerName + "\"" +
                " by \"" + UserName + "\"";
        }

        public AcadCommandTrack()
        {
            UserName = "None";
            ComputerName = "None";
            CommandName = "None";
            CmdExecTime = DateTime.Now;
            DwgFileName = "";
        }
    }
}
