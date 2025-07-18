using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

class Initialization : IExtensionApplication
{
    static AppSettings _settings = null;

    public void Initialize()
    {
        Application.DisplayingOptionDialog += new TabbedDialogEventHandler(Application_DisplayingOptionDialog);
    }

    public void Terminate()
    {
        Application.DisplayingOptionDialog -= new TabbedDialogEventHandler(Application_DisplayingOptionDialog);
    }

    private static void OnOK()
    {
        _settings.Save();
    }

    private static void OnCancel()
    {
        _settings = AppSettings.Load();
    }

    private static void OnHelp()
    {
        // Not currently doing anything here
    }

    private static void OnApply()
    {
        _settings.Save();
    }

    private static void Application_DisplayingOptionDialog(object sender, TabbedDialogEventArgs e)
    {
        if (_settings == null)
            _settings = AppSettings.Load();

        if (_settings != null)
        {
            OptionsDlg.OptionsTabControl otc =
                new OptionsDlg.OptionsTabControl();
            otc.propertyGrid.SelectedObject = _settings;
            otc.propertyGrid.Update();

            TabbedDialogExtension tde =
                new TabbedDialogExtension(
                    otc,
                    new TabbedDialogAction(OnOK),
                    new TabbedDialogAction(OnCancel),
                    new TabbedDialogAction(OnHelp),
                    new TabbedDialogAction(OnApply)
                    );
            e.AddTab("My Application Settings", tde);
        }
    }
}