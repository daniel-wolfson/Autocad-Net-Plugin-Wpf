using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using ID.Infrastructure.Commands;
using ID.Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class PlotExtensions
    {
        private static string PlotFileNamePc3 => CommandNames.UserGroup + "_plot.pc3";
        public static int PlotWidth = 910;

        public static PreviewEndPlotStatus PlotOrPreview(this PlotEngine pe, PlotInfo pi, bool isPreview, int copies = 1, string tempPlotFullPath = null)
        {
            PreviewEndPlotStatus statusResult;
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            tempPlotFullPath = tempPlotFullPath ?? Plugin.Settings.TempPath + "previewed-plot";

            using (doc.LockDocument())
            {
                // Create a Progress Dialog to provide info and allow thej user to cancel
                PlotProgressDialog ppd = new PlotProgressDialog(isPreview, 1, true);

                try
                {
                    ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle,
                        $"{CommandNames.UserGroup} {(isPreview ? "Preview" : "Plot")} Progress");
                    ppd.set_PlotMsgString(PlotMessageIndex.SheetName,
                        doc.Name.Substring(doc.Name.LastIndexOf("\\") + 1));
                    ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                    ppd.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                    ppd.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                    ppd.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");

                    // Let's start the plot/preview, at last
                    ppd.OnBeginPlot();
                    ppd.IsVisible = true;
                    pe.BeginPlot(ppd, null);

                    // We'll be plotting/previewing a single document
                    pe.BeginDocument(pi, doc.Name, null, copies, isPreview, tempPlotFullPath);

                    // Which contains a single sheet
                    ppd.OnBeginSheet();
                    ppd.LowerSheetProgressRange = 0;
                    ppd.UpperSheetProgressRange = 100;
                    ppd.SheetProgressPos = 0;

                    PlotPageInfo ppi = new PlotPageInfo();

                    pe.BeginPage(ppi, pi, true, null);
                    pe.BeginGenerateGraphics(null);
                    ppd.SheetProgressPos = 50;
                    pe.EndGenerateGraphics(null);

                    // Finish the sheet
                    PreviewEndPlotInfo pepi = new PreviewEndPlotInfo();
                    pe.EndPage(pepi);
                    statusResult = pepi.Status;

                    ppd.SheetProgressPos = 100;
                    ppd.OnEndSheet();

                    // Finish the document
                    pe.EndDocument(null);

                    // And finish the plot
                    ppd.PlotProgressPos = 100;
                    ppd.OnEndPlot();
                    pe.EndPlot(null);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                    statusResult = PreviewEndPlotStatus.Cancel;
                }
                finally
                {
                    ppd.Dispose();
                }
            }
            return statusResult;
        }

        private static void AssignToLayout(this PlotSettings plSet)
        {
            // Get the current document and database, and start a transaction
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (acDoc.LockDocument())
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Reference the Layout Manager
                LayoutManager acLayoutMgr = LayoutManager.Current;

                // Get the current layout and output its name in the Command Line window
                Layout acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout),
                    OpenMode.ForRead) as Layout;

                // Update the layout
                if (acLayout != null)
                {
                    acLayout.UpgradeOpen();
                    acLayout.CopyFrom(plSet);
                    acLayout.DowngradeOpen();
                    //PropertyInfo prop = acLayout.GetType().GetProperty("PaperOrientation", BindingFlags.Public | BindingFlags.Instance);
                    //if (null != prop && prop.CanWrite)
                    //    prop.SetValue(acLayout, true);
                    //acLayout.CopyFrom(plSet);
                }

                // Save the new objects to the database
                acTrans.Commit();
                //}
                //else
                //{
                //    // Ignore the changes made
                //    acTrans.Abort();
                //}
            }

            // Update the display
            acDoc.Editor.Regen();
        }

        public static bool IsDeviceExist(this PlotSettingsValidator psv, string name)
        {
            //return psv.GetPlotDeviceList().Cast<string>().ToList().Any(dev => dev.Contains(name));
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary plSets = (DBDictionary)tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead);
                return plSets.Contains(name);
            }
        }

        public static void MakeDevice(this PlotSettingsValidator psv, string name)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary plSets = tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead) as DBDictionary;
                DBDictionary vStyles = tr.GetObject(db.VisualStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                Layout layout = (Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead);

                if (plSets != null && !plSets.Contains(CommandNames.UserGroup))
                {
                    var ps = new PlotSettings(layout.ModelType)
                    {
                        ShadePlot = PlotSettingsShadePlotType.AsDisplayed,
                        ShadePlotResLevel = ShadePlotResLevel.Normal,
                        PrintLineweights = false,
                        PlotTransparency = false,
                        PlotPlotStyles = false,
                        DrawViewportsFirst = false
                    };

                    layout.UpgradeOpen();
                    psv.SetPlotConfigurationName(layout, PlotFileNamePc3, null);
                    layout.DowngradeOpen();

                    ps.CopyFrom(layout); //acLayout

                    ps.PlotSettingsName = CommandNames.UserGroup;
                    ps.AddToPlotSettingsDictionary(db);
                    tr.AddNewlyCreatedDBObject(ps, true);
                    tr.Commit();
                }
            }
        }

        public static KeyValuePair<PlotInfo, CustomPlotSettings>? MakePlotInfo(this PlotSettingsValidator psv,
            ObjectId spaceId, PlotType plotType, Extents2d? win = null, Extents2d? margins = null)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            PlotInfo pi;
            CustomPlotSettings closestPlotSettings;

            try
            {
                using (doc.LockDocument())
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // We'll be plotting the current layout
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(spaceId, OpenMode.ForRead);
                    Layout layout = (Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead);

                    // We need a PlotInfo object linked to the layout
                    pi = new PlotInfo { Layout = btr.LayoutId };

                    DBDictionary plSets = (DBDictionary)tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead);
                    DBDictionary vStyles = tr.GetObject(db.VisualStyleDictionaryId, OpenMode.ForRead) as DBDictionary;
                    PlotSettings ps = default(PlotSettings);
                    bool createNew = false;

                    // Reference the Layout Manager
                    // LayoutManager acLayoutMgr = LayoutManager.Current;
                    // Get the current layout and output its name in the Command Line window
                    // Layout acLayout = tr.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout), OpenMode.ForRead) as Layout;

                    // Check to see if the page setup exists
                    if (!plSets.Contains(CommandNames.UserGroup))
                    {
                        createNew = true;

                        ps = new PlotSettings(layout.ModelType)
                        {
                            ShadePlot = PlotSettingsShadePlotType.AsDisplayed,
                            ShadePlotResLevel = ShadePlotResLevel.Normal,
                            PrintLineweights = false,
                            PlotTransparency = false,
                            PlotPlotStyles = false,
                            DrawViewportsFirst = false
                        };

                        layout.UpgradeOpen();
                        psv.SetPlotConfigurationName(layout, PlotFileNamePc3, null);
                        layout.DowngradeOpen();

                        ps.CopyFrom(layout);

                        psv.SetUseStandardScale(ps, true);
                        psv.SetStdScaleType(ps, StdScaleType.StdScale1To1);
                        psv.SetPlotPaperUnits(ps, PlotPaperUnit.Millimeters);
                        psv.SetPlotOrigin(ps, new Point2d(0, 0)); // Set the plot offset

                        // Use only on named layouts - Hide paperspace objects option
                        //plSet.PlotHidden = true; 
                    }
                    else
                    {
                        ps = plSets.GetAt(CommandNames.UserGroup).GetObject(OpenMode.ForWrite) as PlotSettings;
                    }

                    Extents2d extents = new Extents2d();
                    if (plotType == PlotType.Window)
                    {
                        extents = win ?? layout.Extents.ToExtents2d();
                        if (margins != null) extents.Margin((Extents2d)margins);
                    }
                    else if (plotType == PlotType.Layout)
                    {
                        extents = new Extents2d(
                            new Point2d(db.Limmin.X, db.Limmin.Y),
                            new Point2d(db.Limmax.X, db.Limmax.Y));
                    }

                    closestPlotSettings = GetClosestPlotSettings(psv, ps, CommandNames.UserGroup, extents);
                    if (closestPlotSettings == null)
                    {
                        return new KeyValuePair<PlotInfo, CustomPlotSettings>(null, new CustomPlotSettings("Paper not found"));
                    }

                    try
                    {
                        psv.SetPlotType(ps, plotType);
                    }
                    catch (Exception)
                    {
                    }

                    psv.RefreshLists(ps);
                    psv.SetPlotWindowArea(ps, extents);
                    psv.SetUseStandardScale(ps, true);
                    psv.SetStdScaleType(ps, StdScaleType.StdScale1To1);
                    psv.SetPlotPaperUnits(ps, PlotPaperUnit.Millimeters);
                    psv.SetPlotRotation(ps, ps.PlotRotation);
                    psv.SetPlotCentered(ps, false);
                    psv.SetZoomToPaperOnUpdate(ps, true);
                    //psv.SetCustomPrintScale(newSettings, new CustomScale(1, 1.004 * scale));

                    // Set the plot offset
                    psv.SetPlotOrigin(ps, new Point2d(0, 0));

                    // We need to link the PlotInfo to the PlotSettings and then validate it
                    pi.OverrideSettings = ps;
                    PlotInfoValidator piv = new PlotInfoValidator { MediaMatchingPolicy = MatchingPolicy.MatchEnabled };
                    piv.Validate(pi);

                    if (createNew)
                    {
                        ps.PlotSettingsName = CommandNames.UserGroup;
                        ps.AddToPlotSettingsDictionary(db);
                        tr.AddNewlyCreatedDBObject(ps, true);
                        ps.Dispose();
                    }

                    pi.ValidatedSettings.AssignToLayout();

                    // Committing is cheaper than aborting
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                return new KeyValuePair<PlotInfo, CustomPlotSettings>(null, new CustomPlotSettings("Technical error"));
            }

            return new KeyValuePair<PlotInfo, CustomPlotSettings>(pi, closestPlotSettings);
        }

        private static CustomPlotSettings GetClosestPlotSettings(PlotSettingsValidator psv, PlotSettings ps, string userGroup,
            Extents2d win, Extents2d? margins = null)
        {
            List<CustomPlotSettings> plotSettingsResults = new List<CustomPlotSettings>();

            margins = margins ?? new Extents2d(0, 0, 0, 0);

            PlotProgressDialog ppd = new PlotProgressDialog(true, 1, false);
            using (ppd)
            {
                double windowWidth = win.MaxPoint.X - win.MinPoint.X;
                double windowHeight = win.MaxPoint.Y - win.MinPoint.Y;

                int numSheet = 1;
                ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, $"{CommandNames.UserGroup}: select closest paper for size (width={windowWidth} and height={windowHeight})");
                ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");

                ppd.LowerSheetProgressRange = 0;
                ppd.UpperSheetProgressRange = 100;
                ppd.SheetProgressPos = 0;
                ppd.IsVisible = true;

                List<CustomPlotSettings> plotSettings = psv.GetMediaNames(ps, CommandNames.UserGroup, win)
                    .Where(x => x.HasLandscape || !x.HasLandscape && !x.HasPortrait).ToList();

                foreach (var plotSetting in plotSettings)
                {
                    PlotRotation plotRotation = PlotRotation.Degrees000;
                    bool isMatch = false;

                    if (windowWidth <= plotSetting.MediaWidth && windowHeight <= plotSetting.MediaHeight)
                    {
                        isMatch = true;
                        if (!plotSetting.HasLandscape && !plotSetting.HasPortrait && windowWidth > windowHeight)
                            plotRotation = PlotRotation.Degrees090;
                    }
                    else if (windowWidth <= plotSetting.MediaHeight && windowHeight <= plotSetting.MediaWidth)
                    {
                        isMatch = true;

                        if (!plotSetting.HasLandscape && !plotSetting.HasPortrait && windowWidth < windowHeight)
                            plotRotation = PlotRotation.Degrees000;
                        else
                            plotRotation = PlotRotation.Degrees090;

                        var h = plotSetting.MediaHeight;
                        plotSetting.MediaHeight = plotSetting.MediaWidth;
                        plotSetting.MediaWidth = h;
                    }

                    if (isMatch)
                    {
                        ppd.StatusMsgString = $"processing with {plotSetting.MediaValue} ({numSheet} of {plotSettings.Count})";
                        ppd.SheetProgressPos += Convert.ToInt32(100 / plotSettings.Count);
                        Thread.Sleep(500);
                        numSheet++;

                        plotSetting.PlotRotation = plotRotation;
                        plotSettingsResults.Add(plotSetting);
                    }
                }
                ppd.SheetProgressPos = 100;
            }

            if (plotSettingsResults.Any())
            {
                if (plotSettingsResults.Count > 1)
                    plotSettingsResults = plotSettingsResults.OrderBy(x => x.MediaWidth).ThenBy(y => y.MediaHeight).ToList();

                var closestPlotSettings = plotSettingsResults.First();
                string mediaName = closestPlotSettings.PlotRotation == PlotRotation.Degrees090 && closestPlotSettings.HasPortrait
                    ? closestPlotSettings.MediaNamePair
                    : closestPlotSettings.MediaName;

                psv.SetPlotRotation(ps, closestPlotSettings.PlotRotation);
                if (ps.PlotConfigurationName != PlotExtensions.PlotFileNamePc3)
                {
                    try
                    {
                        psv.SetPlotConfigurationName(ps, PlotExtensions.PlotFileNamePc3, mediaName);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                    }

                }
                psv.SetCanonicalMediaName(ps, mediaName);
                return closestPlotSettings;
            }
            return null;
        }

        private static List<CustomPlotSettings> GetMediaNames(this PlotSettingsValidator psv,
            PlotSettings ps, string userGroup, Extents2d win)
        {
            List<CustomPlotSettings> mediaSettings = new List<CustomPlotSettings>();
            int cnt = 0;
            var canonicalMediaNames = psv.GetCanonicalMediaNameList(ps);

            foreach (string mediaName in canonicalMediaNames)
            {
                var mediaValue = psv.GetLocaleMediaName(ps, cnt);

                if (mediaSettings.All(x => x.MediaName != mediaName) && mediaValue.ToUpper().Contains(userGroup) &&
                    mediaValue.ToUpper().Contains("X") && !mediaValue.ToUpper().Contains("OVERSIZE") &&
                    mediaValue.ToUpper().Split('X').Length == 2)
                {
                    var mediaNameItems = mediaValue.ToUpper().Split(' ');
                    var limits = mediaNameItems[0].Replace(userGroup, "").Replace("_", "").Replace(" ", "").Split('X');
                    int mediaHeight; Int32.TryParse(limits[0], out mediaHeight);
                    int mediaWidth; Int32.TryParse(limits[1], out mediaWidth);

                    mediaSettings.Add(
                        new CustomPlotSettings(mediaName, mediaValue, win)
                        {
                            MediaHeight = mediaHeight,
                            MediaWidth = mediaWidth,
                            HasLandscape = mediaValue.ToUpper().Contains("LANDSCAPE"),
                            HasPortrait = mediaValue.ToUpper().Contains("PORTRAIT"),
                            HasOversize = mediaValue.ToUpper().Contains("OVERSIZE")
                        });
                }
                cnt = cnt + 1;
            }

            foreach (var mediaSetting in mediaSettings)
            {
                CustomPlotSettings ms;
                if (mediaSetting.MediaValue.Contains("LANDSCAPE"))
                {
                    ms = mediaSettings.FirstOrDefault(x =>
                        x.MediaValue.Contains(mediaSetting.MediaValue.Split(' ')[0]) &&
                        x.MediaValue.ToUpper().Contains("PORTRAIT"));
                    mediaSetting.HasPortrait = ms != null;
                    mediaSetting.MediaNamePair = ms != null ? ms.MediaName : mediaSetting.MediaName;
                }
                else if (mediaSetting.MediaValue.Contains("PORTRAIT"))
                {
                    ms = mediaSettings.FirstOrDefault(x =>
                        x.MediaValue.Contains(mediaSetting.MediaValue.Split(' ')[0]) &&
                        x.MediaValue.ToUpper().Contains("LANDSCAPE"));
                    mediaSetting.HasLandscape = ms != null;
                    mediaSetting.MediaNamePair = ms != null ? ms.MediaName : mediaSetting.MediaName;
                }
            }

            return mediaSettings;
        }

        public static void SetPlotSettings(this Layout lay, string pageSize, string styleSheet, string device)
        {
            using (var ps = new PlotSettings(lay.ModelType))
            {
                ps.CopyFrom(lay);
                var psv = PlotSettingsValidator.Current;

                // Set the device
                var devs = psv.GetPlotDeviceList();
                if (devs.Contains(device))
                {
                    psv.SetPlotConfigurationName(ps, device, null);
                    psv.RefreshLists(ps);
                }

                // Set the media name/size
                var mns = psv.GetCanonicalMediaNameList(ps);
                if (mns.Contains(pageSize))
                {
                    psv.SetCanonicalMediaName(ps, pageSize);
                }

                // Set the pen settings
                var ssl = psv.GetPlotStyleSheetList();
                if (ssl.Contains(styleSheet))
                {
                    psv.SetCurrentStyleSheet(ps, styleSheet);
                }

                // Copy the PlotSettings data back to the Layout
                var upgraded = false;
                if (!lay.IsWriteEnabled)
                {
                    lay.UpgradeOpen();
                    upgraded = true;
                }

                lay.CopyFrom(ps);
                if (upgraded)
                {
                    lay.DowngradeOpen();
                }
            }
        }


    }
}