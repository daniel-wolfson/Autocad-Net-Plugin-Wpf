using System;

namespace Intellidesk.Data.Models.Cad
{
    public class AcadCommandArgs: EventArgs
    {
        public string ClientName { set; get; }
        public string ClientGroup { set; get; }
        public string ComputerName { set; get; }
        public string CommandName { set; get; }
        public DateTime ExecTime { set; get; }
        public object CommandArgs { set; get; }

        public override string ToString()
        {
            return "Command \"" + CommandName + "\"" +
                   " executed in drawing \"" + CommandArgs + "\"" +
                   " at " + ExecTime.ToLongTimeString() +
                   " on computer \"" + ComputerName + "\"" +
                   " by \"" + ClientName + "\"";
        }

        public AcadCommandArgs()
        {
            ClientName = "Acad";
            ComputerName = "None";
            CommandName = "None";
            ExecTime = DateTime.Now;
        }
    }
}