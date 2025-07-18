using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Core
{
    public class WpfTextBox : RibbonTextBox
    {
        double _baseHeight;
        double _baseWidth;
        double _heightPadding;
        double _widthPadding;
        bool _textChanging = false;

        public WpfTextBox(double width, double height, double widthPadding, double heightPadding)
        {
            // Set some member variables, some of which
            // we also use to set the TextBox dimensions

            _baseWidth = width;
            _baseHeight = height;
            _widthPadding = widthPadding;
            _heightPadding = heightPadding;

            Width = width;
            Height = height;
            MinWidth = width;

            // Register our focus-related event handlers
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent,
                new RoutedEventHandler(OnGotFocus));

            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.LostKeyboardFocusEvent,
                new RoutedEventHandler(OnLostFocus));

            // And out additional TextChanged event handler
            EventManager.RegisterClassHandler(typeof(TextBox), TextBoxBase.TextChangedEvent,
                new RoutedEventHandler(OnTextChanged));
        }

        public string GetTextWithoutNewlines()
        {
            // Return the contained text without newline characters
            return TextValue.ReplaceNewlinesWithSpaces();
        }

        public void ClearText()
        {
            TextValue = "";
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            if (!_textChanging && e != null && e.Source != null)
            {
                var tb = e.Source as TextBox;
                if (tb != null)
                {
                    // We need the typeface to calculate the text width
                    var faces = tb.FontFamily.GetTypefaces();
                    Typeface face = null;
                    foreach (Typeface tf in faces)
                    {
                        if (tf != null)
                        {
                            face = tf;
                            break;
                        }
                    }

                    // Get the last line of text, to see how long it is
                    var text = tb.Text.GetLastLine();

                    // Calculate the width of this last line of text
                    if (face != null)
                    {
                        var ft = new FormattedText(
                            text,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            face,
                            tb.FontSize * (96.0 / 72.0),
                            Brushes.Black
                            );

                        // If the width of the last line of text
                        // is over our boundary (the width of the box
                        // minus some padding), then start the next
                        // line

                        if (ft.Width - _widthPadding > _baseWidth)
                        {
                            // Set our flag to stop re-entry of the event
                            // handler, then replace the last space in the
                            // TextBox contents with a newline

                            _textChanging = true;
                            tb.Text =
                                tb.Text.InsertNewlineAtLastSpace();
                            _textChanging = false;

                            // Set the cursor to be at the end of our text
                            // so that typing continues properly

                            tb.SelectionStart = tb.Text.Length;
                        }
                    }

                    // Find the number of lines of text
                    var lines = tb.Text.GetLineCount();

                    // Change the height based on the number of lines
                    tb.Height = _heightPadding + (lines * _baseHeight);
                    tb.MinLines = lines;
                }
            }
        }

        // Both events call the same helper, with a custom message
        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChange(sender, e, "\nTextbox got focus :)\n");
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChange(sender, e, "\nTextbox lost focus :(\n");
        }

        // Our helper function to print  a message only when
        // our custom textbox exists
        private void OnFocusChange(object sender, RoutedEventArgs e, string msg)
        {
            if (e != null && e.Source != null)
            {
                var tb = e.Source as TextBox;
                if (tb != null)
                {
                    var mtb = tb.DataContext as WpfTextBox;
                    if (mtb != null)
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                    }
                }
            }
        }
    }
}