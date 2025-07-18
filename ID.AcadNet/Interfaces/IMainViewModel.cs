using System.Collections.ObjectModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Intellidesk.AcadNet.Data.Models;
using Rule = Intellidesk.AcadNet.Data.Models.Rule;
using UserSetting = Intellidesk.AcadNet.Data.Models.UserSetting;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IProjectExlorerViewModel
    {
        /// <summary> LayoutsContentsItems </summary>
        string[] LayoutsContentsItems { get; set; }
        ObservableCollection<Config> ConfigItems { get; }
        ObservableCollection<Layout> LayoutItems { get; }
        ObservableCollection<Rule> RuleItems { get; }
        ObservableCollection<Filter> LayoutFilterItems { get; }
        ObservableCollection<UserSetting> UserSettingItems { get; }
        ObservableCollection<RibbonButton> RibbonLayoutItems { get; }
        Layout CurrentLayout { get; set; }

        void OnDocumentActivated(object sender, DocumentCollectionEventArgs e);
    }
}