using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Markup;
using Intellidesk.Common.Extensions;

namespace Intellidesk.Common.Enums
{
    // ReSharper disable once InconsistentNaming

    public class EnumTypeToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumTypeToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            switch (_type.Name)
            {
                case "eCableType":
                    return Enum.GetValues(_type)
                        .Cast<eCableType>()
                        .Select(e => new
                        {
                            Value = e,
                            DisplayName = e.ToString(),
                            Color = e.GetMetaDataInfo().ColorIndex
                        });
                default:
                    return new List<object>() { new { Value = 0, DisplayName = "Demo"}};

            }

            
        }
    }
}