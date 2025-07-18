using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.Extentions
{
    public static class EditExtensions
    {
        private static ObjectId toolTipObjectId = ObjectId.Null;

        //ComponentManager.ToolTipOpened += ComponentManager_ToolTipOpened;

        public static void WriteMessageAsync(this Editor editor, string message)
        {
            acadApp.DocumentManager.ExecuteCommandAsync(new CommandArgs(null, CommandNames.XWriteMessage, message, false));
        }

        public static void ComponentManager_ToolTipOpened(object sender, EventArgs e)
        {
            if (toolTipObjectId != ObjectId.Null && acadApp.GetSystemVariable("RollOverTips").ToString() == "1")
            {
                var toolTip = sender as ToolTip;

                // This check is needed to distinguish between the ribbon tooltips and the entity tooltips
                if (toolTip != null)
                {
                    object member = null;
                    using (
                        Transaction transaction =
                            HostApplicationServices.WorkingDatabase.TransactionManager.StartOpenCloseTransaction())
                    {
                        member = transaction.GetObject(toolTipObjectId, 0) as object;
                        transaction.Commit();
                    }
                    if (member != null)
                    {
                        var memberToolTip = new SuperToolTipDisplay
                        {
                            //MaxWidth = 600,
                            //ClassName = { Text = "Object Type" }
                        };


                        // Repeat this section for each property of the object to add to the tooltip
                        // I added generic text for this sample but you would instead get properties from the object
                        // and display them here.  The blockName would be the name of the property and the blockValue
                        // would be the value of the property.
                        {
                            var blockName = new TextBlock();
                            var blockValue = new TextBlock();

                            blockName.Text = "Property Name";
                            blockName.Margin = new Thickness(0.0, 5.0, 0.0, 0.0);
                            blockName.HorizontalAlignment = HorizontalAlignment.Left;
                            blockName.VerticalAlignment = VerticalAlignment.Center;

                            blockValue.Text = "Property Value";

                            blockValue.Margin = new Thickness(10.0, 5.0, 10.0, 0.0);
                            blockValue.TextWrapping = TextWrapping.Wrap;
                            blockValue.HorizontalAlignment = HorizontalAlignment.Left;
                            blockValue.VerticalAlignment = VerticalAlignment.Center;

                            //memberToolTip.StackPanelName.Children.Add(blockName);
                            //memberToolTip.StackPanelValue.Children.Add(blockValue);

                            // Because BlockValue textblock can have wrapped text we need to set the height
                            // of the BlockName textblock to equal that of the BlockValue textblock.
                            // We need to give the wpf layout engine time to calculate the actual height
                            // so that we can set the values.

                            //memberToolTip.StackPanelValue.Dispatcher.BeginInvoke(
                            //    DispatcherPriority.Background,
                            //    new DispatcherOperationCallback(delegate
                            //    {
                            //        blockName.Height = blockValue.ActualHeight;
                            //        return null;
                            //    }), null);
                        }



                        // Swap out the AutoCAD ToolTip with our own ToolTip
                        toolTip.Content = memberToolTip;
                        //member.Dispose();
                    }
                }

                // Reset the object for the next tooltip
                toolTipObjectId = ObjectId.Null;
            }
        }

        public static void Editor_Rollover(object sender, RolloverEventArgs e)
        {
            if (!e.Highlighted.IsNull)
            {
                ObjectId[] objectsIds = e.Highlighted.GetObjectIds();
                if ((objectsIds != null) && (objectsIds.Length > 0))
                {
                    if ((objectsIds[0].ObjectClass.Name == "AecbDbDuct") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbDuctFitting") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbDuctCustomFitting") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbDuctFlex") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbPipe") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbPipeFitting") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbPipeCustomFitting") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbPipeFlex") ||
                      (objectsIds[0].ObjectClass.Name == "AecbDbMvPart"))
                    {
                        toolTipObjectId = objectsIds[0];
                    }
                    else
                    {
                        toolTipObjectId = ObjectId.Null;
                    }
                }
                else
                {
                    toolTipObjectId = ObjectId.Null;
                }
            }
            else
            {
                toolTipObjectId = ObjectId.Null;
            }
        }

        //Matrix3d cs = context.Ed.CurrentUserCoordinateSystem;
        //_MAPCSASSIGN
    }
}