using System;
using System.IO;
using System.ServiceProcess;

namespace ID.AppHost
{
    public partial class AppHostService : ServiceBase
    {
        public AppHostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //AddLog("start");
            File.AppendAllText(@"c:\temp\Intellidesk\AppHostService.txt", $"{DateTime.Now} started{Environment.NewLine}");
        }

        protected override void OnStop()
        {
            //AddLog("stop");
            File.AppendAllText(@"c:\temp\Intellidesk\AppHostService.txt", $"{DateTime.Now} stopped{Environment.NewLine}");
        }

        public void AddLog(string log)
        {
            //try
            //{
            //    if (!EventLog.SourceExists("ID.AppHost"))
            //    {
            //        EventLog.CreateEventSource("ID.AppHostService", "ID.AppHostService");
            //    }
            //    eventLog1.Source = "MyExampleService";
            //    eventLog1.WriteEntry(log);
            //}
            //catch { }
        }
    }

}
