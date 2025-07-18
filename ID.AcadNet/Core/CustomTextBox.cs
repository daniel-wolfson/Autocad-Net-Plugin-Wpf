using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Core
{
    public delegate void LinkPressedEvent(object sender, LinkPressedEventArgs args);

    public class CustomTextBlockLinks : TextBlock
    {
        static CustomTextBlockLinks()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomTextBoxLinks), new FrameworkPropertyMetadata(typeof(CustomTextBoxLinks)));
        }

        public CustomTextBlockLinks()
        {
            TargetUpdated += OnTargetUpdated;

            if (DesignerProperties.GetIsInDesignMode(this)) return;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomTextBlockLinks), new FrameworkPropertyMetadata(typeof(CustomTextBlockLinks)));
        }

        private void OnTargetUpdated(object sender, DataTransferEventArgs dataTransferEventArgs)
        {
            FormatText(this);
        }

        private void FormatText(TextBlock tb)
        {
            var style = LinkStyle;
            if (tb != null)
            {

                var str = tb.Text;
                var hasLinks = true;
                var pos = 0;
                var previosPos = 0;

                while (pos < str.Length - 3)
                {
                    if (str.Substring(pos, 3) == " ")
                    {
                        if (hasLinks)
                            tb.Inlines.Clear();
                        var previosSpacePos = pos;
                        var nextSpacePos = pos;

                        while (str[previosSpacePos] != ' ' && previosSpacePos > 0)
                            previosSpacePos--;
                        while (str[nextSpacePos] != ' ' && nextSpacePos < str.Length - 1)
                            nextSpacePos++;

                        var linkStr = str.Substring(previosSpacePos, nextSpacePos - previosSpacePos + 1);
                        if (previosSpacePos != previosPos)
                            tb.Inlines.Add(str.Substring(previosPos, previosSpacePos - previosPos));

                        var run = new Run(linkStr) { Style = style };
                        run.MouseDown += RunOnMouseDown;
                        tb.Inlines.Add(run);

                        previosPos = nextSpacePos;
                        pos = nextSpacePos;
                        hasLinks = false;
                    }
                    if (pos + 1 == str.Length - 3)
                        tb.Inlines.Add(str.Substring(previosPos, str.Length - previosPos));
                    pos++;
                }
            }
        }

        private void RunOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var r = sender as Run;
            if (r == null) return;
            var l = r.Text;
            OnLinkPressed(new LinkPressedEventArgs(l));
        }

        public event LinkPressedEvent LinkPressed;

        public void OnLinkPressed(LinkPressedEventArgs args)
        {
            var handler = LinkPressed;
            if (handler != null) handler(this, args);
        }

        public static readonly DependencyProperty LinkStyleProperty = DependencyProperty.Register("LinkStyle", typeof(Style), typeof(CustomTextBlockLinks));

        public Style LinkStyle
        {
            get { return (Style)GetValue(LinkStyleProperty); }
            set { SetValue(LinkStyleProperty, value); }
        }
    }

    public class LinkPressedEventArgs : EventArgs
    {
        public readonly string Link;

        public LinkPressedEventArgs(string link)
        {
            Link = link;
        }
    }
}
