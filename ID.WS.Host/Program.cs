using System.ServiceProcess;

namespace ID.WS.Host
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new AppHostService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
