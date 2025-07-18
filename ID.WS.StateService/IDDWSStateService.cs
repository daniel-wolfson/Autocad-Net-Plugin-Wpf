using System.ServiceModel;
using System.ServiceProcess;

namespace ID.WS.StateService
{
    partial class IDWSStateService : ServiceBase
    {
        private ServiceHost _autorizationManager;
        public IDWSStateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            CheckAndClose(ref _autorizationManager);
            _autorizationManager = new ServiceHost(typeof(StateService));
            _autorizationManager.Open();
        }

        protected override void OnStop()
        {
            CheckAndClose(ref _autorizationManager);
        }

        private static void CheckAndClose(ref ServiceHost host)
        {
            if (host != null)
            {
                host.Close();
                host = null;
            }
        }
    }
}
