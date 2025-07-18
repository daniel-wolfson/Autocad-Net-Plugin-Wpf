using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Jig;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class EditorExtensions
    {
        #region <Promts>

        public static PromptResult XPromptYesNo(this Editor ed, string prmpt, bool answerBydefault)
        {
            string answerBydefaultStr = answerBydefault ? "Yes" : "No";
            PromptKeywordOptions prOpts = new PromptKeywordOptions(prmpt);
            prOpts.Keywords.Add("Yes");
            prOpts.Keywords.Add("No");
            prOpts.Keywords.Default = answerBydefaultStr;

            PromptResult promptResult = ed.GetKeywords(prOpts);
            //if (prRes.Status == PromptStatus.OK)
            //{
            //    if (prRes.StringResult == "Yes")
            //        answer = true;
            //}
            return promptResult;
        }

        public static PromptEntityResult XPromptForObject(this Editor ed, string promptMessage, Type allowedType, bool exactMatchOfAllowedType)
        {
            var polyOptions = new PromptEntityOptions(promptMessage);
            polyOptions.SetRejectMessage("Entity is not of type " + allowedType);
            polyOptions.AddAllowedClass(allowedType, exactMatchOfAllowedType);
            PromptEntityResult polyResult = ed.GetEntity(polyOptions);
            return polyResult;
        }

        public static PromptPointResult XPromptForPoint(this Editor ed, string promptMessage, bool useDashedLine = false, bool useBasePoint = false, Point3d basePoint = new Point3d(), bool allowNone = true)
        {
            var pointOptions = new PromptPointOptions(promptMessage);
            if (useBasePoint)
            {
                pointOptions.UseBasePoint = true;
                pointOptions.BasePoint = basePoint;
                pointOptions.AllowNone = allowNone;
            }

            if (useDashedLine)
            {
                pointOptions.UseDashedLine = true;
            }
            PromptPointResult pointResult = ed.GetPoint(pointOptions);
            return pointResult;
        }

        public static PromptPointResult XPromptForPoint(this Editor ed, PromptPointOptions promptPointOptions)
        {
            return ed.GetPoint(promptPointOptions);
        }

        public static PromptDoubleResult XPromptForDouble(this Editor ed, string promptMessage, double defaultValue = 0.0)
        {
            var doubleOptions = new PromptDoubleOptions(promptMessage);
            if (Math.Abs(defaultValue - 0.0) > Double.Epsilon)
            {
                doubleOptions.UseDefaultValue = true;
                doubleOptions.DefaultValue = defaultValue;
            }
            PromptDoubleResult promptDoubleResult = ed.GetDouble(doubleOptions);
            return promptDoubleResult;
        }

        public static PromptIntegerResult XPromptForInteger(this Editor ed, string promptMessage)
        {
            PromptIntegerResult promptIntResult = ed.GetInteger(promptMessage);
            return promptIntResult;
        }

        public static PromptIntegerResult XPromptForInteger(this Editor ed, string promptMessage,
            bool bIntAllowArbitraryInput, bool bIntAllowNone, bool bIntAllowZero, bool bIntAllowNegative,
            bool bIntUseDefault, int def)
        {
            PromptIntegerOptions prOpts = new PromptIntegerOptions(promptMessage);
            prOpts.AllowArbitraryInput = bIntAllowArbitraryInput;
            prOpts.AllowNone = bIntAllowNone;
            prOpts.AllowZero = bIntAllowZero;
            prOpts.AllowNegative = bIntAllowNegative;
            prOpts.UseDefaultValue = bIntUseDefault;
            if (bIntUseDefault)
                prOpts.DefaultValue = def;

            PromptIntegerResult promptIntResult = ed.GetInteger(prOpts);
            return promptIntResult;
        }

        public static PromptResult XPromptForKeywordSelection(this Editor ed, string promptMessage,
            IEnumerable<string> keywords, bool allowNone, string defaultKeyword = "")
        {
            var promptKeywordOptions = new PromptKeywordOptions(promptMessage) { AllowNone = allowNone };
            foreach (var keyword in keywords)
            {
                promptKeywordOptions.Keywords.Add(keyword);
            }
            if (defaultKeyword != "")
            {
                promptKeywordOptions.Keywords.Default = defaultKeyword;
            }
            PromptResult keywordResult = ed.GetKeywords(promptKeywordOptions);
            return keywordResult;
        }

        public static Point3dCollection XPromptForRectangle(this Editor ed, string promptMessage, out PromptStatus status)
        {
            var resultRectanglePointCollection = new Point3dCollection();
            var viewCornerPointResult = ed.XPromptForPoint(promptMessage);
            var pointPromptStatus = viewCornerPointResult.Status;
            if (viewCornerPointResult.Status == PromptStatus.OK)
            {
                var rectangleJig = new RectangleJig(viewCornerPointResult.Value);
                var jigResult = ed.Drag(rectangleJig);
                if (jigResult.Status == PromptStatus.OK)
                {
                    // remove duplicate point at the end of the rectangle
                    var polyline = rectangleJig.Polyline;
                    var viewPolylinePoints = polyline.XGetPoints(); // GeometryUtility.GetPointsFromPolyline(polyline);
                    if (viewPolylinePoints.Count == 5)
                        viewPolylinePoints.RemoveAt(4); // dont know why but true, probably mirror point with the last point
                }
                pointPromptStatus = jigResult.Status;
            }
            status = pointPromptStatus;
            return resultRectanglePointCollection;
        }

        public static PromptSelectionResult XPromptForSelection(this Editor ed, string promptMessage = null,
            SelectionFilter filter = null)
        {
            var selectionOptions = new PromptSelectionOptions { MessageForAdding = promptMessage };
            PromptSelectionResult selectionResult = String.IsNullOrEmpty(promptMessage)
                ? ed.SelectAll(filter)
                : ed.GetSelection(selectionOptions, filter);
            return selectionResult;
        }

        public static PromptSelectionResult XPromptForSelection(this Editor ed, PromptSelectionOptions promptSelectionOptions,
            SelectionFilter filter = null)
        {
            return ed.GetSelection(promptSelectionOptions, filter);
        }

        // get count Point or listPoints with promptMessage to user
        public static PromptPointsResult GetPoints(this Editor ed, string message = null, int? nAttemptMax = null, bool isWindow = false)
        {
            ed.SetImpliedSelection(new ObjectId[] { }); //new ObjectId[0]; new ObjectId[] {ent.ObjectId}

            PromptPointResult ppr;
            var pointsList = new List<Point3d>();
            var ppo = new PromptPointOptions("") { AllowNone = true };
            message = string.IsNullOrEmpty(message) ? "" : message + ". ";

            var nAttempt = 1;
            do
            {
                if (nAttempt == 1)
                {
                    ppo.Message = $"{message}Enter the {(nAttemptMax > 2 ? "start" : "first")} {(isWindow ? "corner" : "point")}: ";
                    ppr = ed.GetPoint(ppo);
                }
                else
                {
                    if (isWindow)
                    {
                        PromptCornerOptions pco =
                            new PromptCornerOptions($"{message}Select second corner: ", ppo.BasePoint);
                        ppr = ed.GetCorner(pco);
                    }
                    else
                    {
                        ppo.Message = $"{message}Enter {(nAttemptMax > 2 ? "next" : "second")} point <{nAttempt}>: ";
                        ppr = ed.GetPoint(ppo);
                    }
                }

                if (ppr.Status == PromptStatus.Cancel)
                    break;

                if (ppr.Status == PromptStatus.OK || ppr.Status == PromptStatus.None)
                {
                    ppo.UseBasePoint = true;
                    ppo.BasePoint = ppr.Value;

                    pointsList.Add(ppr.Status == PromptStatus.None
                        ? ppo.BasePoint
                        : new Point3d(Convert.ToInt32(ppr.Value.X), Convert.ToInt32(ppr.Value.Y), ppr.Value.Z));

                    int colorIndex = 1;
                    foreach (var pnt in pointsList)
                    {
                        using (ResultBuffer resBuf = new ResultBuffer())
                        {
                            resBuf.Add(new TypedValue(1001, colorIndex));
                            // The actual vector
                            resBuf.Add(new TypedValue(1001, new Point2d(pnt.X, pnt.Y)));
                            // and its mirror about X axis
                            resBuf.Add(new TypedValue(1001, new Point2d(pnt.X, -pnt.Y)));
                            ed.DrawVectors(resBuf, Matrix3d.Identity);
                        }
                    }

                    if (nAttempt > 1 && ppo.BasePoint == ppr.Value) break;
                }

                if (nAttemptMax != null && nAttemptMax == nAttempt) break;

                nAttempt = nAttempt + 1;
            } while (true);

            //ICommandLine commandLine = Plugin.GetService<ICommandLine>();
            //commandLine.Cancel();

            return new PromptPointsResult(ppr.Status, pointsList);
        }

        #endregion <Promts>

        #region <Selects>


        /// <summary> Select objects with N attempt</summary>
        public static List<ObjectId> GetSelectionPrompt(this Editor ed, int nAttemptMax, string message = null, bool escapeEnabled = true)
        {
            int nAttempt = 0;
            SelectionSet curSet = null;
            ed.SetImpliedSelection(new ObjectId[] { }); //new ObjectId[0]; new ObjectId[] {ent.ObjectId}
            var commandLine = Plugin.GetService<ICommandLine>();
            message = string.IsNullOrEmpty(message) ? "Select objects:" : message;

            do
            {
                var psr = ed.GetSelection(new PromptSelectionOptions()
                {
                    MessageForAdding = commandLine.Current() + (escapeEnabled ? message + " | Esc" : message)
                });

                if (psr.Status == PromptStatus.OK)
                {
                    curSet = psr.Value; //ss.GetObjectIds.CopyTo(ids, 0)
                    IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
                    ed.WriteMessage(pluginSettings.Prompt + "Objects selected: " + Convert.ToString(curSet.Count));
                    break;
                }

                nAttempt = nAttempt + 1;
                if (nAttempt == nAttemptMax) break;
                ed.WriteMessage(commandLine.Current() + "Objects selected: 0");
            } while (true);

            return curSet != null ? curSet.GetObjectIds().ToList() : null;
        }


        /// <summary> Get implied objects </summary>
        public static List<ObjectId> GetSelectImplied(this Editor ed)
        {
            var result = new List<ObjectId>();
            var psr = ed.SelectImplied();
            if (psr.Status == PromptStatus.OK)
                result = psr.Value.GetObjectIds().ToList();
            return result;
        }

        #endregion <Selects>

        public static void WriteMessage(this Editor ed, string message)
        {
            ed.WriteMessage(message);
        }

        public static void DrawVector(this Editor ed, Point3d from, Point3d to, int color, bool drawHighlighted)
        {
            ed.DrawVector(from, to, color, drawHighlighted);
        }
    }
}