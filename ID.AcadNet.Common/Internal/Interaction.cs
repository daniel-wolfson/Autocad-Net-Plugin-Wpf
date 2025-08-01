﻿using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Windows;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Drawing;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Jig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ColorDialog = Autodesk.AutoCAD.Windows.ColorDialog;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Intellidesk.AcadNet.Common.Internal
{
    /// <summary>
    /// Command-line user interactions.
    /// </summary>
    public static class Interaction
    {
        /// <summary>
        /// Gets the MDI active docutment's editor.
        /// </summary>
        public static Editor ActiveEditor
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        /// <summary>
        /// Writes message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Write(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(message);
        }

        /// <summary>
        /// Writes message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Write(string message, params object[] args)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(message, args);
        }

        /// <summary>
        /// Writes message line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteLine(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n");
            ed.WriteMessage(message);
        }

        /// <summary>
        /// Writes message line.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteLine(string message, params object[] args)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n");
            ed.WriteMessage(message, args);
        }

        /// <summary>
        /// Gets string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The string.</returns>
        public static string GetString(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetString(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }

            return null;
        }

        /// <summary>
        /// Gets string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The string.</returns>
        public static string GetString(string message, string defaultValue)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetString(new PromptStringOptions(message) { DefaultValue = defaultValue });
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }

            return null;
        }

        /// <summary>
        /// Gets keywords.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="defaultIndex">The default index.</param>
        /// <returns>The keyword result.</returns>
        public static string GetKeywords(string message, string[] keywords, int defaultIndex = 0)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptKeywordOptions(message)
            {
                AllowNone = true
            }; // mod 20140527

            keywords.ForEach(key => opt.Keywords.Add(key));
            opt.Keywords.Default = keywords[defaultIndex];

            var res = ed.GetKeywords(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }

            return null;
        }

        /// <summary>
        /// Gets numeric value.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        public static double GetValue(string message, double? defaultValue = null)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = defaultValue == null
                ? ed.GetDouble(new PromptDoubleOptions(message) { AllowNone = true })
                : ed.GetDouble(new PromptDoubleOptions(message) { DefaultValue = defaultValue.Value, AllowNone = true });

            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return double.NaN;
        }

        /// <summary>
        /// Gets distance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The distance.</returns>
        public static double GetDistance(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetDistance(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return double.NaN;
        }

        /// <summary>
        /// Gets angle.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The angle.</returns>
        public static double GetAngle(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetAngle(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return double.NaN;
        }

        /// <summary>
        /// Gets point.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The point.</returns>
        public static Point3d GetPoint(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptPointOptions(message)
            {
                AllowNone = true
            };

            var res = ed.GetPoint(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return Algorithms.NullPoint3d;
        }

        /// <summary>
        /// Gets another point of a line.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="startPoint">The first point.</param>
        /// <returns>The point.</returns>
        public static Point3d GetLineEndPoint(string message, Point3d startPoint)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new LineJig2(startPoint, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.EndPoint;
            }

            return Algorithms.NullPoint3d;
        }

        /// <summary>
        /// Get corner point.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="basePoint">The first point.</param>
        /// <returns>The point.</returns>
        public static Point3d GetCorner(string message, Point3d basePoint)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptCornerOptions(message, basePoint) { AllowNone = true }; // mod 20140527
            var res = ed.GetCorner(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return Algorithms.NullPoint3d;
        }

        /// <summary>
        /// Gets 2-d extents.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The extents.</returns>
        public static Extents3d? GetExtents(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetPoint(message);
            if (res.Status != PromptStatus.OK)
            {
                return null;
            }

            var p1 = res.Value;
            res = ed.GetCorner(message, p1);
            if (res.Status != PromptStatus.OK)
            {
                return null;
            }

            var p2 = res.Value;
            return new Extents3d(
                new Point3d(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0),
                new Point3d(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0));
        }

        /// <summary>
        /// Gets entity.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId GetEntity(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetEntity(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId;
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Gets entity.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="allowedType">The allowed entity type.</param>
        /// <param name="exactMatch">Use exact match.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId GetEntity(string message, Type allowedType, bool exactMatch = true) // newly 20130514
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptEntityOptions(message);
            opt.SetRejectMessage("Allowed type: " + allowedType.Name); // Must call this first
            opt.AddAllowedClass(allowedType, exactMatch);
            var res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId;
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Gets entity and pick position.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID and the pick position.</returns>
        public static Tuple<ObjectId, Point3d> GetPick(string message)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetEntity(message);
            if (res.Status == PromptStatus.OK)
            {
                return Tuple.Create(res.ObjectId, res.PickedPoint);
            }

            return null;
        }

        /// <summary>
        /// Gets multiple entities.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetSelection(string message, params TypedValue[] filterList)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptSelectionOptions { MessageForAdding = message };
            ed.WriteMessage(message);
            var res = filterList != null && filterList.Any()
                ? ed.GetSelection(opt, new SelectionFilter(filterList))
                : ed.GetSelection(opt);

            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return new ObjectId[0];
        }

        /// <summary>
        /// Gets multiple entities.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetSelection(string message, FilterList filterList)
        {
            return GetSelection(message, filterList.ToArray());
        }

        /// <summary>
        /// Gets multiple entities.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="allowedType">The allowed types. e.g. "*LINE,ARC,CIRCLE"</param>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetSelection(string message, string allowedType)
        {
            return GetSelection(message, new TypedValue((int)DxfCode.Start, allowedType));
        }

        /// <summary>
        /// Gets multiple entities by window.
        /// </summary>
        /// <param name="pt1">The corner 1.</param>
        /// <param name="pt2">The corner 2.</param>
        /// <param name="allowedType">The allowed types.</param>
        /// <returns>The selection.</returns>
        public static ObjectId[] GetWindowSelection(Point3d pt1, Point3d pt2, string allowedType = "*")
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectWindow(pt1, pt2, new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, allowedType) }));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return new ObjectId[0];
        }

        /// <summary>
        /// Gets multiple entities by crossing.
        /// </summary>
        /// <param name="pt1">The corner 1.</param>
        /// <param name="pt2">The corner 2.</param>
        /// <param name="allowedType">The allowed types.</param>
        /// <returns>The selection.</returns>
        public static ObjectId[] GetCrossingSelection(Point3d pt1, Point3d pt2, string allowedType = "*")
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectCrossingWindow(pt1, pt2, new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, allowedType) }));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return new ObjectId[0];
        }

        /// <summary>
        /// Gets the pick set.
        /// </summary>
        /// <remarks>
        /// Don't forget to add CommandFlags.UsePickSet to CommandMethod.
        /// </remarks>
        /// <returns>The selection.</returns>
        public static ObjectId[] GetPickSet()
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectImplied();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return new ObjectId[0];
        }

        /// <summary>
        /// Sets the pick set.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void SetPickSet(ObjectId[] entityIds)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.SetImpliedSelection(entityIds);
        }

        /// <summary>
        /// Gets the last added entity.
        /// </summary>
        /// <returns>The entity ID.</returns>
        public static ObjectId GetNewestEntity()
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectLast();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds()[0];
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Gets the last added entities.
        /// </summary>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetNewestEntities()
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectLast();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return new ObjectId[0];
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr child, IntPtr parent);

        /// <summary>
        /// Sets focus to the active document.
        /// </summary>
        public static void SetActiveDocFocus()
        {
            SetFocus(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Window.Handle);
        }

        /// <summary>
        /// Sets the current layer.
        /// </summary>
        /// <param name="layerName">The layer name.</param>
        public static void SetCurrentLayer(string layerName)
        {
            HostApplicationServices.WorkingDatabase.Clayer = DbHelper.GetLayerId(layerName);
        }

        /// <summary>
        /// Sends command to execute.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void Command(string command)
        {
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(command, false, false, false);
        }

        /// <summary>
        /// Starts new command.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void StartCommand(string command)
        {
            var existingCommands = Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CMDNAMES").ToString();
            var escapes = existingCommands.Length > 0
                ? string.Join(string.Empty, Enumerable.Repeat("\x03", existingCommands.Split('\'').Length).ToArray())
                : string.Empty;

            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(escapes + command, true, false, true);
        }

        /// <summary>
        /// Shows a task dialog.
        /// </summary>
        /// <param name="mainInstruction">The main instruction.</param>
        /// <param name="yesChoice">The yes choice.</param>
        /// <param name="noChoice">The no choice.</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="content">The content.</param>
        /// <param name="footer">The footer.</param>
        /// <param name="expanded">The expanded text.</param>
        /// <returns>The choice.</returns>
        public static bool TaskDialog(string mainInstruction, string yesChoice, string noChoice, string title = "AutoCAD", string content = "", string footer = "", string expanded = "")
        {
            var td = new TaskDialog
            {
                WindowTitle = title,
                MainInstruction = mainInstruction,
                ContentText = content,
                MainIcon = TaskDialogIcon.Information,
                FooterIcon = TaskDialogIcon.Warning,
                FooterText = footer,
                CollapsedControlText = "Details",
                ExpandedControlText = "Details",
                ExpandedByDefault = false,
                ExpandedText = expanded,
                AllowDialogCancellation = false,
                UseCommandLinks = true
            };
            td.Buttons.Add(new TaskDialogButton(1, yesChoice));
            td.Buttons.Add(new TaskDialogButton(2, noChoice));
            td.DefaultButton = 1;
            int[] btnId = null;
            td.Callback = (atd, e, sender) =>
            {
                if (e.Notification == TaskDialogNotification.ButtonClicked)
                {
                    btnId = new int[3];
                    btnId[e.ButtonId] = 1;
                }
                return false;
            };
            td.Show(Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle);
            if (btnId.ToList().IndexOf(1) == 1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Highlights entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void HighlightObjects(IEnumerable<ObjectId> entityIds)
        {
            entityIds.XForEach<Entity>(entity => entity.Highlight());
        }

        /// <summary>
        /// Unhighlights entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void UnhighlightObjects(IEnumerable<ObjectId> entityIds)
        {
            entityIds.XForEach<Entity>(entity => entity.Unhighlight());
        }

        /// <summary>
        /// Zooms to entities' extents.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void ZoomObjects(IEnumerable<ObjectId> entityIds)
        {
            var extent = entityIds.GetExtents();
            ZoomView(extent);
        }

        /// <summary>
        /// Zooms to extents.
        /// </summary>
        /// <param name="extents">The extents.</param>
        public static void ZoomView(Extents3d extents)
        {
            Zoom(extents.MinPoint, extents.MaxPoint, new Point3d(), 1);
        }

        /// <summary>
        /// Zooms to all entities' extents.
        /// </summary>
        public static void ZoomExtents()
        {
            if (HostApplicationServices.WorkingDatabase.TileMode) // Model space
            {
                Zoom(HostApplicationServices.WorkingDatabase.Extmin, HostApplicationServices.WorkingDatabase.Extmax, new Point3d(), 1);
            }
            else // Paper space
            {
                Zoom(new Point3d(), new Point3d(), new Point3d(), 1);
            }
        }

        /// <summary>
        /// The internal Zoom() method (credit: AutoCAD .NET Developer's Guide).
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="center"></param>
        /// <param name="factor"></param>
        internal static void Zoom(Point3d min, Point3d max, Point3d center, double factor)
        {
            // Get the current document and database
            var document = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            int currentViewport = Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CVPORT"));

            // Get the extents of the current space no points
            // or only a center point is provided
            // Check to see if Model space is current
            if (database.TileMode)
            {
                if (min.Equals(new Point3d()) && max.Equals(new Point3d()))
                {
                    min = database.Extmin;
                    max = database.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if (currentViewport == 1)
                {
                    // Get the extents of Paper space
                    if (min.Equals(new Point3d()) && max.Equals(new Point3d()))
                    {
                        min = database.Pextmin;
                        max = database.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (min.Equals(new Point3d()) && max.Equals(new Point3d()))
                    {
                        min = database.Extmin;
                        max = database.Extmax;
                    }
                }
            }

            // Start a transaction
            using (var trans = database.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (var currentView = document.Editor.GetCurrentView())
                {
                    Extents3d extents;

                    // Translate WCS coordinates to DCS
                    var matWCS2DCS = Matrix3d.PlaneToWorld(currentView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(currentView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(
                        angle: -currentView.ViewTwist,
                        axis: currentView.ViewDirection,
                        center: currentView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max
                    // point of the extents
                    // for Center and Scale modes
                    if (center.DistanceTo(Point3d.Origin) != 0)
                    {
                        min = new Point3d(center.X - currentView.Width / 2, center.Y - currentView.Height / 2, 0);
                        max = new Point3d(currentView.Width / 2 + center.X, currentView.Height / 2 + center.Y, 0);
                    }

                    // Create an extents object using a line
                    using (Line line = new Line(min, max))
                    {
                        extents = new Extents3d(line.Bounds.Value.MinPoint, line.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double viewRatio = currentView.Width / currentView.Height;

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    extents.TransformBy(matWCS2DCS);

                    double width;
                    double height;
                    Point2d newCenter;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (center.DistanceTo(Point3d.Origin) != 0)
                    {
                        width = currentView.Width;
                        height = currentView.Height;

                        if (factor == 0)
                        {
                            center = center.TransformBy(matWCS2DCS);
                        }

                        newCenter = new Point2d(center.X, center.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        width = extents.MaxPoint.X - extents.MinPoint.X;
                        height = extents.MaxPoint.Y - extents.MinPoint.Y;

                        // Get the center of the view
                        newCenter = new Point2d(
                            (extents.MaxPoint.X + extents.MinPoint.X) * 0.5,
                            (extents.MaxPoint.Y + extents.MinPoint.Y) * 0.5);
                    }

                    // Check to see if the new width fits in current window
                    if (width > height * viewRatio)
                    {
                        height = width / viewRatio;
                    }

                    // Resize and scale the view
                    if (factor != 0)
                    {
                        currentView.Height = height * factor;
                        currentView.Width = width * factor;
                    }

                    // Set the center of the view
                    currentView.CenterPoint = newCenter;

                    // Set the current view
                    document.Editor.SetCurrentView(currentView);
                }

                // Commit the changes
                trans.Commit();
            }
        }

        /// <summary>
        /// Inserts entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId InsertEntity(Entity entity, string message = "\nSpecify insert point")
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new PositionJig(entity, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Inserts scaling entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId InsertScalingEntity(Entity entity, Point3d basePoint, string message = "\nSpecify diagonal point")
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new ScaleJig(entity, basePoint, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Inserts rotation entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="center">The center.</param>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId InsertRotationEntity(Entity entity, Point3d center, string message = "\nSpecify direction")
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new RotationJig(entity, center, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Shows OS save file dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="filter">The type filter.</param>
        /// <returns>The file name result.</returns>
        public static string SaveFileDialogBySystem(string title, string fileName, string filter)
        {
            var sfd = new SaveFileDialog
            {
                Title = title,
                FileName = fileName,
                Filter = filter
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                return sfd.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows OS open file dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="filter">The type filter.</param>
        /// <returns>The file name result.</returns>
        public static string OpenFileDialogBySystem(string title, string fileName, string filter)
        {
            var ofd = new OpenFileDialog
            {
                Title = title,
                FileName = fileName,
                Filter = filter
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// The OS folder dialog.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>The folder result.</returns>
        public static string FolderDialog(string description)
        {
            var fbd = new FolderBrowserDialog
            {
                Description = description,
                RootFolder = Environment.SpecialFolder.Desktop,
                ShowNewFolderButton = true
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }

            return string.Empty;
        }

        // TODO: file dialog by AutoCAD
        //public static void SaveFileDialogByAutoCAD()
        //{
        //}

        /// <summary>
        /// Shows AutoCAD color dialog.
        /// </summary>
        /// <returns>The color result.</returns>
        public static Color ColorDialog()
        {
            var cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                return cd.Color;
            }

            return null;
        }

        /// <summary>
        /// Creates polyline interactively.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The polyline result.</returns>
        public static Polyline GetPromptPolyline(string message) // newly 20130806
        {
            var point = GetPoint(message);
            if (point.IsNull())
            {
                return null;
            }
            var poly = DrawObject.Pline(new[] { point });
            var prev = point;
            var tempIds = new List<ObjectId>();
            while (true)
            {
                point = GetLineEndPoint(message, prev);
                if (point.IsNull())
                {
                    break;
                }
                tempIds.Add(Drawing.Drawing.Line(prev, point));
                poly.AddVertexAt(poly.NumberOfVertices, point.ToPoint2d(), 0, 0, 0);
                prev = point;
            }
            tempIds.XForEach(line => line.Erase());
            return poly;
        }

        /// <summary>
        /// View entities interactively.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="action">The action.</param>
        public static void ZoomHighlightView(List<ObjectId> entityIds, Action<int> action = null) // newly 20130815
        {
            if (entityIds.Count > 0)
            {
                var highlightIds = new List<ObjectId>();
                while (true)
                {
                    string input = GetString("\nType in a number to view, press ENTER to exit: ");
                    if (input == null)
                    {
                        break;
                    }
                    var index = Convert.ToInt32(input);
                    if (index <= 0 || index > entityIds.Count)
                    {
                        WriteLine("Invalid entity number.");
                        continue;
                    }

                    action?.Invoke(index);
                    highlightIds.Clear();
                    highlightIds.Add(entityIds[index - 1]);
                    ZoomObjects(highlightIds);
                    HighlightObjects(highlightIds);
                }
            }
        }

        /// <summary>
        /// Starts a FlexEntityJig drag.
        /// </summary>
        /// <typeparam name="TOptions">The type of JigPromptOptions.</typeparam>
        /// <typeparam name="TResult">The type of jig PromptResult.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag<TOptions, TResult>(TOptions options, Entity entity, Func<Entity, TResult, bool> updateAction)
            where TOptions : JigPromptOptions
            where TResult : PromptResult
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new FlexEntityJig(options, entity, (ent, result) => updateAction(ent, (TResult)result));
            return ed.Drag(jig);
        }

        /// <summary>
        /// Starts a FlexEntityJig point drag.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag(string message, Entity entity, Func<Entity, PromptPointResult, bool> updateAction)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var options = new JigPromptPointOptions(message); // TODO: other options?
            var jig = new FlexEntityJig(options, entity, (ent, result) => updateAction(ent, (PromptPointResult)result));
            return ed.Drag(jig);
        }

        /// <summary>
        /// Starts a FlexDrawJig drag.
        /// </summary>
        /// <typeparam name="TOptions">The type of JigPromptOptions.</typeparam>
        /// <typeparam name="TResult">The type of jig PromptResult.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag<TOptions, TResult>(TOptions options, Func<TResult, Drawable> updateAction)
            where TOptions : JigPromptOptions
            where TResult : PromptResult
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new FlexDrawJig(options, result => updateAction((TResult)result));
            return ed.Drag(jig);
        }

        /// <summary>
        /// Starts a FlexDrawJig point drag.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag(string message, Func<PromptPointResult, Drawable> updateAction)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var options = new JigPromptPointOptions(message); // TODO: other options?
            var jig = new FlexDrawJig(options, result => updateAction((PromptPointResult)result));
            return ed.Drag(jig);
        }
    }
}
