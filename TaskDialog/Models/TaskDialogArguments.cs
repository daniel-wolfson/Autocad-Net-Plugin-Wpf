using Intellidesk.Common.Enums;
using Intellidesk.Data.General;
using System;
using System.Linq;
using System.Media;
using System.Xml;
using TaskDialogInterop.Interfaces;

namespace TaskDialogInterop.Models
{
    public class TaskDialogArguments : TaskArguments
    {
        #region Dialog

        public new IActiveTaskDialog Dialog { get; set; }

        /// <summary> show status in dialog </summary>
        /// <param name="timerTickCount">get timerTick from ActiveTaskDialog for display timer</param>
        public new bool DisplayStatus(uint timerTickCount)
        {
            var result = false;
            switch (Status)
            {
                case StatusOptions.Start:
                    //_taskbarManager.SetProgressState(TaskbarProgressBarState.Normal);
                    //Dialog.SetButtonEnabledState(500, false);
                    //Dialog.UpdateMainIcon(VistaTaskDialogIcon.Shield);
                    ExpandedInfo = "";
                    Status = StatusOptions.Runnig;
                    ProgressPercentage = 0;
                    result = true;
                    break;

                case StatusOptions.Runnig:
                    //Dialog.SetWindowTitle(string.Format("{0:P0} Complete performance...", Convert.ToDouble(ProgressPercentage) / 100d));

                    if (ProgressPercentage == 100 || ProgressIndex == ProgressLimit)
                    {
                        //completion the processes
                        Status = StatusOptions.Done;
                        Content = "Actions have been successful";

                        var xmlDoc = new XmlDocument();
                        var root = xmlDoc.AppendChild(xmlDoc.CreateElement("Errors"));
                        if (CommandInfo.Any())
                        {
                            ExpandedInfo += "\nErrors: " + CommandInfo.Count();
                            Dialog.SetExpandedInformation(ExpandedInfo);
                            foreach (var info in CommandInfo)
                            {
                                var child = root.AppendChild(xmlDoc.CreateElement("Error"));
                                var childAtt = child.Attributes.Append(xmlDoc.CreateAttribute("Message"));
                                childAtt.InnerText = info.Value; //Child.InnerText = err;
                            }
                        }
                        try
                        {
                            xmlDoc.Save(AppSettings.UserSettingsPath + "Errors.xml");
                        }
                        catch (Exception) { }
                    }

                    var content = IsTimerOn
                        ? string.Format("Time elapsed: {0} | {1}", TimeSpan.FromMilliseconds(timerTickCount).ToString(@"h\:mm\:ss"), Content)
                        : string.Format("{0}", Content);
                    //Dialog.SetProgressBarPosition(Percent);
                    Dialog.SetContent(content);
                    Dialog.SetExpandedInformation(ExpandedInfo);
                    break;

                case StatusOptions.Done:
                    SystemSounds.Asterisk.Play();
                    //_taskbarManager.SetProgressState(TaskbarProgressBarState.Paused);
                    Dialog.UpdateMainIcon(VistaTaskDialogIcon.SecuritySuccess);
                    Status = StatusOptions.None;
                    break;

                case StatusOptions.Cancel:
                    if (Dialog != null)
                    {
                        Dialog.UpdateMainIcon(VistaTaskDialogIcon.SecurityError);
                        Content = "Action has been canceled by user";
                        Dialog = null;
                        //if (taskThread != null) taskThread.Abort();
                        //worker = null;
                        Status = StatusOptions.None;
                    }
                    break;

                case StatusOptions.Error:
                    if (Dialog != null)
                    {
                        SystemSounds.Hand.Play();
                        Dialog.UpdateMainIcon(VistaTaskDialogIcon.SecurityError);
                        Dialog.ClickCustomButton(1); //dialog.ClickButton(TaskDialog.GetButtonIdForCustomButton(1));
                        Status = StatusOptions.None;
                        result = true;
                        //Log.Logger.Error(new Exception(ExpandedInfo), "");
                        //_taskbarManager.SetProgressState(TaskbarProgressBarState.Error);
                    }
                    break;

                default:
                    result = true;
                    break;
            }
            return result;
        }

        #endregion
    }
}
