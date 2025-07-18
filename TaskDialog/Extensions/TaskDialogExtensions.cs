using Intellidesk.Common.Enums;

namespace TaskDialogInterop.Extensions
{
    public static class TaskDialogExtensions
    {

        //[DllImport("comctl32.dll", CharSet = CharSet.Unicode, EntryPoint = "TaskDialog")]
        //public static extern int TaskDialogW32(IntPtr hWndParent, IntPtr hInstance, String pszWindowTitle,
        //                                     String pszMainInstruction, String pszContent, int dwCommonButtons, IntPtr pszIcon, out int pnButton);

        private static bool _neveragain = false;

        public static TaskDialog XTaskInit(this TaskDialog td) //, TaskFunc taskFunc
        {
            var done = false;
            while (!done)
            {
                var decision = StatusOptions.Start;
                if (!_neveragain)
                {
                    //var td = new TaskDialog
                    //             {
                    //                 Width = 200,
                    //                 WindowTitle = "Many objects selected",
                    //                 MainInstruction = psr.Value.Count.ToString("") + " objects have been selected, " +
                    //                                   "which may lead to a time-consuming operation.",
                    //                 UseCommandLinks = true,
                    //                 VerificationText = "Do not ask me this again"
                    //             };
                    //td.CallbackData = new TaskData();
                    //td.Buttons.Add(new TaskDialogButton(0, "Run the operation on the selected objects."));
                    //td.Buttons.Add(new TaskDialogButton(1, "Cancel")); //Do nothing and cancel the command

                    //td.Callback = delegate(ActiveTaskDialog atd, TaskDialogCallbackArgs e, object sender) //TaskDialogEventArgs
                    //                  {
                    //                      if (e.Notification == TaskDialogNotification.ButtonClicked)
                    //                      {
                    //                          switch (e.ButtonId)
                    //                          {
                    //                              case 0:
                    //                                  decision = DecisionOptions.Continue;
                    //                                  //var handler = ToolsTaskEvent;
                    //                                  //if (handler != null) done = handler(atd, e);
                    //                                  break;
                    //                              case 1:
                    //                                  decision = DecisionOptions.ReStart;
                    //                                  var handler = ToolsTaskEvent;
                    //                                  if (handler != null) done = handler(atd, e);
                    //                                  break;
                    //                              case 2:
                    //                                  decision = DecisionOptions.Cancel;
                    //                                  return true;
                    //                                  break;
                    //                          }
                    //                      }
                    //if (e.Notification == TaskDialogNotification.VerificationClicked)
                    //{
                    //    _neveragain = true;
                    //}
                    //return false;
                    //                  };

                    switch (decision)
                    {
                        case StatusOptions.Continue:
                            // Perform the operation anyway...
                            //Ed.WriteMessage("\nThis is where we do something " + "with our {0} entities.", psr.Value.Count);
                            //var handler = ToolsTaskEvent;
                            //if (handler != null) done = handler((ActiveTaskDialog)atd, new EventArgs()); //(TaskData)td.CallbackData
                            break;

                        case StatusOptions.Init:
                            done = false;
                            break;

                        case StatusOptions.Cancel:
                            done = false;
                            break;

                        // Includes DecisionOptions.Start...
                        default:
                            done = true;
                            break;
                    }
                }
            }

            return td;
        }

        public static void TaskDialogOptions1()
        {
            //OK – 1, Cancel – 8, Yes – 2, No – 4, Retry – 10, Close – 20
            // Create the task dialog itself
            var td = new TaskDialog
            {
                //WindowTitle = "The title",
                //MainInstruction = "Something has happened.",
                //ContentText = "Here's some text, with a " + "<A HREF=\"http://adn.autodesk.com\">" + "link to the ADN site</A>",
                //VerificationText = "Verification text",
                //FooterText = "The footer with a " + "<A HREF=\"http://blogs.autodesk.com/through" + "-the-interface\">link to Kean's blog</A>",
                //EnableHyperlinks = true,
                //EnableVerificationHandler = true,
                //CollapsedControlText = "This control text can be expanded.",
                //ExpandedControlText = "This control text has been expanded..." + "\nTo span multiple lines.",
                //ExpandedText = "This footer text has been expanded.",
                //ExpandFooterArea = true,
                //ExpandedByDefault = false,
                //MainIcon = TaskDialogIcon.Shield,
                //FooterIcon = TaskDialogIcon.Information,
                //ShowProgressBar = true,
                //UseCommandLinks = true
            };

            // Set the various textual settings
            // And those for collapsed/expanded text
            // Set some standard icons and display of the progress bar
            // A marquee progress bas just loops,
            // it has no range  fixed upfront
            //td.ShowMarqueeProgressBar = true;
            // Now we add out task action buttons

            //td.Buttons.Add(new TaskDialogButton(1, "This is one course of action."));
            //td.Buttons.Add(new TaskDialogButton(2, "Here is another course of action."));
            //td.Buttons.Add(new TaskDialogButton(3, "And would you believe we have a third!"));

            //// Set the default to be the third
            //td.DefaultButton = 3;

            //// And some radio buttons, too
            //td.RadioButtons.Add(new TaskDialogButton(4, "Yes"));
            //td.RadioButtons.Add(new TaskDialogButton(5, "No"));
            //td.RadioButtons.Add(new TaskDialogButton(6, "Maybe"));

            //// Set the default to be the second
            //td.DefaultRadioButton = 5;

            //// Allow the dialog to be cancelled
            //td.AllowDialogCancellation = false;

            //// Implement a callback for UI event notification
            //td.Callback = delegate(ActiveTaskDialog atd, TaskDialogCallbackArgs args, object sender)
            //{
            //    Ed.WriteMessage("\nButton ID: {0}", args.ButtonId);
            //    Ed.WriteMessage("\nNotification: {0}", args.Notification);

            //    if (args.Notification == TaskDialogNotification.VerificationClicked)
            //    {
            //        atd.SetProgressBarRange(0, 100);
            //        atd.SetProgressBarPosition(80);
            //    }

            //    else if (args.Notification == TaskDialogNotification.HyperlinkClicked)
            //    {
            //        Ed.WriteMessage(" " + args.Hyperlink);
            //    }
            //    Ed.WriteMessage("\n");

            //    // Returning true will prevent the dialog from
            //    // being closed
            //    return false;
            //};

            //td.Show(acadApp.MainWindow.Handle);
        }
    }
}
