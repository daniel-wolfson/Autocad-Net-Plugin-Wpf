using ID.Infrastructure;
using Intellidesk.Data.Models.Cad;
using System.Windows;
using System.Windows.Controls;

namespace Intellidesk.AcadNet.ViewModels
{
    class LayoutSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var layout = item as ILayout;
            var templateKey = "SinglePerson";

            if (layout == null)
            {
                return base.SelectTemplate(item, container);
            }

            // Die hier benutzten Templates sind in XAML definiert
            IMapViewModel viewModel = Plugin.GetService<IMapViewModel>();
            templateKey = viewModel.ToggleLayoutDataTemplateSelector == 1
                ? "SinglePerson"
                : "NonSinglePerson";

            var personControl = container as UserControl;
            if (personControl != null)
                return personControl.FindResource(templateKey) as DataTemplate;

            return null;
        }
    }
}