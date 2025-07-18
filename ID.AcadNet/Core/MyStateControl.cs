using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Intellidesk.AcadNet.Core
{
    public class MyStateControl : ButtonBase
    {
        public MyStateControl() : base() { }
        public Boolean State
        {
            get { return (Boolean)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }
        public readonly DependencyProperty StateProperty = DependencyProperty.Register(
            "State", typeof(Boolean), typeof(MyStateControl), new PropertyMetadata(false));
    }
}