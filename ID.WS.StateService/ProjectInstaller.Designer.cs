namespace ID.WS.StateService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.IDWsStateServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.IDWsStateServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // IDWsStateServiceProcessInstaller
            // 
            this.IDWsStateServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.IDWsStateServiceProcessInstaller.Password = null;
            this.IDWsStateServiceProcessInstaller.Username = null;
            // 
            // IDWsStateServiceInstaller
            // 
            this.IDWsStateServiceInstaller.Description = "Not Licenced";
            this.IDWsStateServiceInstaller.DisplayName = "IntelliDesk StateService";
            this.IDWsStateServiceInstaller.ServiceName = "IDWsStateService";
            this.IDWsStateServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.IDWsStateServiceProcessInstaller,
            this.IDWsStateServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller IDWsStateServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller IDWsStateServiceInstaller;
    }
}