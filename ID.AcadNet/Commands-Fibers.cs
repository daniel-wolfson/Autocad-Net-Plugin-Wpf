#define PARTNER

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Drawing;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Jig;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using TextHorizontalMode = Autodesk.AutoCAD.DatabaseServices.TextHorizontalMode;


#if (PARTNER)
[assembly: CommandClass(typeof(CommandFibers))]
#endif
//[assembly: CommandClass(typeof(Intellidesk.AcadNet.Commands.CommandsJig))]
namespace Intellidesk.AcadNet
{
    /// <summary>
    /// This class is instantiated by AutoCAD for each document when
    /// a command is called by the user the first time in the context
    /// of a given document. In other words, non static data in this class
    /// is implicitly per-document!
    /// </summary>
    public class CommandFibers : CommandDocumentBase
    {
        private readonly IDrawService _drawService = Plugin.GetService<IDrawService>();
        private readonly ILayerService _layerService = Plugin.GetService<ILayerService>();

        #region <Cable>

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.Cable, CommandFlags.Session)]
        public void CableOpen()
        {
            Linetypes.AddCableLinetype(eCableType.Cable144x12x12);
            ICommandArgs palleteCommandArgs = new CommandArgs(null, CommandNames.Cable, new AcadCable(eCableType.Cable12x1x12));
            ToolsManager.LoadPallete(PaletteNames.Cable, palleteCommandArgs, null);
        }

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddCable, CommandFlags.Session)]
        public void CableAdd()
        {
            var panelContext = Plugin.GetService<ICablePanelContext>();
            if (panelContext == null) return;

            var objIds = MakeCable(panelContext.ElementDataContext.CurrentElement);
            if (objIds != null)
            {
                panelContext.IsLoaded = false;
                panelContext.ElementItems.Clear();
                panelContext.ElementItems.AddRange(objIds);
                panelContext.IsLoaded = true;
            }
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.UpdateCable, CommandFlags.Session | CommandFlags.NoHistory)]
        public void CableUpdate()
        {
            var panelContext = Plugin.GetService<ICablePanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.ElementDataContext.CurrentElement;
            ownerElement.ObjectState = ObjectState.Edit;

            DBObject dbObject = Db.XGetObject(ownerElement.Handle);
            dbObject.XUpgradeOpen(ownerElement);

            panelContext.ElementItems.Clear();
            panelContext.ElementItems.AddRange(Db.XGetObjectDisplayItems(ownerElement.Handle));
        }

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddTitleCable, CommandFlags.Session)]
        public void CableAddTitle()
        {
            var panelContext = Plugin.GetService<ICablePanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.ElementDataContext.CurrentElement;

            DBObject entityOwner = Db.XGetObject(ownerElement.Handle);
            IPaletteElement elementOwner = entityOwner.XGetDataObject();
            if (entityOwner == null || elementOwner == null) return;

            elementOwner.Title = ownerElement.Title;

            var titleItems = MakeTitle(elementOwner, ((Entity)entityOwner).XGetMidPoint());
            if (titleItems != null)
                panelContext.ElementItems.AddRange(titleItems);

            entityOwner.ObjectId.XOpenForWrite(ent => ent.XAddData(elementOwner));
        }

        [CommandAspect]
        [CommandMethod(CommandNames.MainGroup, CommandNames.CablePanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void CablePanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.Cable];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        [CommandAspect]
        [CommandMethod(CommandNames.MainGroup, CommandNames.CabinetPanelRmove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void CabinetPanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.Cabinet];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        /// <summary> Cable </summary>
        public IEnumerable<ObjectIdItem> MakeCable(AcadCable acadCable)
        {
            acadApp.SetSystemVariable("SNAPMODE", 1);
            List<ObjectIdItem> displayItems = new List<ObjectIdItem>();

            try
            {
                Point3d basePoint;
                ObjectId ownerObjectId;

                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    var service = new PolylineJigService();
                    var ownerEntity = service.Draw(false, true, acadCable.ColorIndex.HasValue ? (short)acadCable.ColorIndex : (short)0);

                    if (ownerEntity == null || ownerEntity.NumberOfVertices == 0) return null;

                    //ObjectId linetypeId;
                    //Linetypes.CableTypeCache.TryGetValue((eCableType)acadCable.TypeId, out linetypeId);
                    //if (linetypeId != ObjectId.Null)
                    //    ownerEntity.LinetypeId = linetypeId;
                    // ownerEntity //.TransformBy(jig.Ucs);

                    ownerEntity.Color = Color.FromColorIndex(ColorMethod.ByAci, acadCable.ColorIndex.HasValue ? (short)acadCable.ColorIndex : (short)0);
                    //pline.SetStoredRotation(0);

                    var currentLayerName = acadCable.TypeCode.GetDataInfo().LayerName;

                    if (!_layerService.Contains(currentLayerName))
                        _layerService.Add(currentLayerName);

                    ownerEntity.Layer = currentLayerName;
                    ownerEntity.XSaveChanges();
                    ownerObjectId = ownerEntity.ObjectId;

                    basePoint = ownerEntity.GetPoint3dAt(ownerEntity.NumberOfVertices / 2);
                    acadCable.BasePoint = basePoint;
                    acadCable.Handle = ownerEntity.Handle.ToString();
                    acadCable.OwnerHandle = ownerEntity.Handle.ToString();
                }

                IEnumerable<ObjectIdItem> titleDisplayItems = MakeTitle(acadCable, basePoint);

                ownerObjectId.XOpenForWrite(ent =>
                {
                    ent.XAddData(acadCable);
                    ObjectIdItem item = ent.XGetDisplayItem(acadCable);
                    displayItems.Add(item);
                });

                if (titleDisplayItems.Any())
                    displayItems.AddRange(titleDisplayItems);

            }
            catch (System.Exception ex)
            {
                //ownerEntity?.XErase();
                //titleText?.XErase();
                Doc.Editor.WriteMessage(PluginSettings.Prompt + " MakeCable error: " + ex);
            }
            finally
            {
                Selects.Clean();
            }

            return displayItems;
        }

        #endregion

        #region <Closure>

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.Closure, CommandFlags.Session)]
        public void ClosureOpen()
        {
            //var commandParams = new CommandParams() { ParamAction = "New", ParamTypeId = eClosureType.CL };
            var commandArgs = new CommandArgs(null, CommandNames.Closure, new AcadClosure(eClosureType.Cl));
            ToolsManager.LoadPallete(PaletteNames.Closure, commandArgs, null);
        }

        [CommandAspect]
        [CommandMethod(CommandNames.MainGroup, CommandNames.ClosurePanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ClosurePanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.Closure];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.AddClosure, CommandFlags.Session)]
        public void ClosureAdd()
        {
            var panelContext = Plugin.GetService<IClosurePanelContext>();
            if (panelContext == null) return;

            //Db.InsertBlock("closure", Point3d.Origin, new BlockOptions { Scale = 1, JigPrompt = eJigPrompt.PromptInsert });
            var objIdItems = MakeClosure(panelContext.BodyElementDataContext.CurrentElement);
            if (objIdItems != null)
            {
                panelContext.IsLoaded = false;
                panelContext.ElementItems.Clear();
                panelContext.ElementItems.AddRange(objIdItems);
                panelContext.IsLoaded = true;
            }
        }

        [CommandAspect]
        [CommandMethod(CommandNames.MainGroup, CommandNames.UpdateClosure, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ClosureUpdate()
        {
            var panelContext = Plugin.GetService<IClosurePanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.BodyElementDataContext.CurrentElement;
            ownerElement.ObjectState = ObjectState.Edit;
            DBObject dbObject = Db.XGetObject(ownerElement.Handle);
            dbObject.XUpgradeOpen(ownerElement);

            panelContext.ElementItems.Clear();
            panelContext.ElementItems.AddRange(Db.XGetObjectDisplayItems(ownerElement.Handle));
        }

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddTitleClosure, CommandFlags.Session)]
        public void ClosureAddTitle()
        {
            var panelContext = Plugin.GetService<IClosurePanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.BodyElementDataContext.CurrentElement;

            DBObject entityOwner = Db.XGetObject(ownerElement.Handle);
            IPaletteElement elementOwner = entityOwner.XGetDataObject();
            if (entityOwner == null || elementOwner == null) return;

            elementOwner.Title = ownerElement.Title;

            var titleItems = MakeTitle(elementOwner, ((Entity)entityOwner).XGetMidPoint());
            if (titleItems != null)
                panelContext.ElementItems.AddRange(titleItems);

            entityOwner.ObjectId.XOpenForWrite(ent =>
            {
                ent.XAddData(elementOwner);
            });
        }

        /// <summary> Closure( </summary>
        public IEnumerable<ObjectIdItem> MakeClosure(AcadClosure acadClosure)
        {
            acadApp.SetSystemVariable("SNAPMODE", 1);
            List<ObjectIdItem> displayItems = new List<ObjectIdItem>();

            try
            {
                Point3d basePoint;
                ObjectId ownerObjectId;

                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    ElementEntityJig jig = new ElementEntityJig(
                        new Circle(Point3d.Origin, Vector3d.ZAxis, 3.5), acadClosure);

                    PromptResult promptResult = jig.Drag(eDragType.Location);
                    basePoint = jig.BasePoint;

                    if (promptResult.Status == PromptStatus.Cancel || promptResult.Status == PromptStatus.Error)
                        return null;

                    var currentLayerName = acadClosure.TypeCode.GetDataInfo().LayerName;

                    if (!_layerService.Contains(currentLayerName))
                        _layerService.Add(currentLayerName);

                    var draw = Plugin.GetService<IDrawService>();
                    Entity donut = DrawObject.Donut(basePoint.X, basePoint.Y, 6, 8, 2);
                    donut.Layer = currentLayerName;
                    donut.XSaveChanges();

                    var circle = new Circle
                    {
                        Center = basePoint,
                        Radius = 3.5,
                        Color = Colors.Black,
                        Layer = currentLayerName
                    };
                    circle.XSaveChanges();
                    ownerObjectId = circle.ObjectId;

                    var ownerEntity = Db.CreateGroup(acadClosure, new[] { donut.ObjectId, circle.ObjectId });

                    acadClosure.BasePoint = circle.Center;
                    acadClosure.Handle = circle.Handle.ToString();
                    acadClosure.OwnerHandle = circle.Handle.ToString();

                    donut.XAddData(acadClosure);
                    circle.XAddData(acadClosure);
                }

                IEnumerable<ObjectIdItem> titleDisplayItems = MakeTitle(acadClosure, basePoint);

                ownerObjectId.XOpenForWrite(ent =>
                {
                    ent.XAddData(acadClosure);
                    ObjectIdItem item = ent.XGetDisplayItem(acadClosure);
                    displayItems.Add(item);
                });

                if (titleDisplayItems.Any())
                    displayItems.AddRange(titleDisplayItems);
            }
            catch (System.Exception ex)
            {
                string message = $"{PluginSettings.Prompt}Draw closure error: {ex}, {ex.Source}";
                Doc.Editor.WriteMessage(message);
                Plugin.Logger.Error($"{nameof(MakeClosure)} error: ", ex);
            }
            finally
            {
                Selects.Clean();
            }

            return displayItems;
        }

        #endregion

        #region <ClosureConnect>

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.ClosureConnect, CommandFlags.Session)]
        public void ClosureConnectOpen()
        {
            //var commandParams = new CommandParams() { ParamAction = "New", ParamTypeId = eClosureType.CL };
            var commandArgs = new CommandArgs(null, CommandNames.ClosureConnect,
                new PaletteElement()
                {
                    ElementType = typeof(ClosureConnect).FullName,
                    ElementName = typeof(ClosureConnect).Name,
                    TypeCodeFullName = typeof(eOpenCloseType).FullName,
                    TypeCode = 0,
                    ColorIndex = 0
                });

            ToolsManager.LoadPallete(PaletteNames.ClosureConnect, commandArgs, null);
        }

        [CommandAspect]
        [CommandMethod(CommandNames.MainGroup, CommandNames.ClosureConnectPanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ClosureConnecPanelRemove()
        {
            ToolsManager.PaletteTabs.CloseTab(PaletteNames.ClosureConnect);
        }

        [CommandAspect]
        //[CommandMethod(CommandNames.UserGroup, CommandNames.AddClosureConnect, CommandFlags.Interruptible | CommandFlags.InProgress | CommandFlags.NoHistory)]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddClosureConnect, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ClosureConnectAdd()
        {
            acadApp.SetSystemVariable("SNAPMODE", 1);

            var panelContext = Plugin.GetService<IClosureConnectPanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.BodyElementDataContext.CurrentElement;
            Point3d basePoint = Point3d.Origin;

            // Get point for Body
            eOpenCloseType eOpenClose = (eOpenCloseType)ownerElement.TypeCode;
            var CurrentLayerName = eOpenClose.GetDataInfo().LayerName;

            if (!_layerService.Contains(CurrentLayerName))
                _layerService.Add(CurrentLayerName);

            PromptPointResult pr = Doc.Editor.GetPoint(new PromptPointOptions("\nFirst corner:"));
            if (pr.Status != PromptStatus.OK) return;
            basePoint = pr.Value;

            // Draw Body
            var bodyItems = MakeBody(eBodyType.Rectangle, ownerElement, basePoint)?.ToList();
            if (bodyItems == null || !bodyItems.Any()) return;

            var ownerObjectId = bodyItems.First().ObjectId;
            List<ObjectIdItem> ownerItems = new List<ObjectIdItem>();

            // Draw Title
            var titleItems = MakeTitle(ownerElement, basePoint, 1);
            if (titleItems.Any())
                ownerItems.AddRange(titleItems);

            // Draw Marker
            if (ownerElement.TypeCode > 0)
            {
                var markerElement = panelContext.MarkerElementDataContext.CurrentElement;

                var markerItems = MakeMarker(markerElement, ownerElement, basePoint);
                if (markerItems.Any())
                    ownerItems.AddRange(markerItems);
            }

            // Update panel context
            if (ownerItems != null && ownerItems.Any())
            {
                ownerObjectId.XOpenForWrite(ownerItems);

                panelContext.IsLoaded = false;
                panelContext.MarkerElementDataContext.ElementItems.ForEach(el =>
                {
                    el.OwnerHandle = ownerElement.Handle;
                    el.OwnerFullType = ownerElement.OwnerFullType;
                });

                panelContext.ElementItems.Clear();
                panelContext.ElementItems.AddRange(bodyItems);
                panelContext.ElementItems.AddRange(ownerItems);
                panelContext.RunButtonText = "Update";
                panelContext.IsLoaded = true;
            }
        }

        [CommandAspect]
        [CommandMethod(CommandNames.MainGroup, CommandNames.UpdateClosureConnect, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ClosureConnectUpdate()
        {
            var panelContext = Plugin.GetService<IClosureConnectPanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.BodyElementDataContext.CurrentElement;

            //var currentElement1 = dataContext.BodyElementDataContext.CurrentElement;
            ownerElement.ObjectState = ObjectState.Edit;
            DBObject dbObject = Db.XGetObject(ownerElement.Handle);

            if (dbObject != null)
            {
                dbObject.XUpgradeOpen(ownerElement);
                panelContext.ElementItems.Clear();
                var items = Db.XGetObjectDisplayItems(ownerElement.Handle);
                panelContext.ElementItems.AddRange(items);
            }
        }

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddTitleClosureConnect, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ClosureConnectAddTitle()
        {
            var panelContext = Plugin.GetService<IClosureConnectPanelContext>();
            if (panelContext != null)
            {
                var ownerElement = panelContext.BodyElementDataContext.CurrentElement;

                DBObject entityOwner = Db.XGetObject(ownerElement.Handle);
                IPaletteElement elementOwner = entityOwner.XGetDataObject();
                if (entityOwner == null || elementOwner == null) return;

                elementOwner.Title = ownerElement.Title;
                IEnumerable<ObjectIdItem> titleItems = MakeTitle(elementOwner, ((Entity)entityOwner).XGetMidPoint());
                if (titleItems != null)
                {
                    panelContext.ElementItems.AddRange(titleItems);
                    entityOwner.ObjectId.XOpenForWrite(ent => ent.XAddData(elementOwner));
                }
            }
        }

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddMarkerClosureConnect, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ClosureConnectAddMarker()
        {
            var panelContext = Plugin.GetService<IClosureConnectPanelContext>();
            if (panelContext != null)
            {
                var ownerElement = panelContext.BodyElementDataContext.CurrentElement;

                IPaletteElement elementOwner = null;

                DBObject entityOwner = Db.XGetObject(ownerElement.OwnerHandle, OpenMode.ForRead, (ent) =>
                    elementOwner = ent.XGetDataObject()
                );

                if (entityOwner == null || elementOwner == null) return;

                var markerElement = panelContext.MarkerElementDataContext.CurrentElement;

                var markerItems = MakeMarker(markerElement, elementOwner, ((Entity)entityOwner).XGetMidPoint());
                if (markerItems != null)
                {
                    panelContext.ElementItems.AddRange(markerItems);
                }

                entityOwner.ObjectId.XOpenForWrite(ent => ent.XAddData(elementOwner));


            }
        }

        /// <summary> Closure( </summary>
        public List<ObjectIdItem> MakeClosureConnect(IPaletteElement ownerElement, out Point3d basePoint)
        {
            acadApp.SetSystemVariable("SNAPMODE", 1);
            List<ObjectIdItem> displayItems = new List<ObjectIdItem>();
            basePoint = Point3d.Origin;

            try
            {
                eOpenCloseType eOpenClose = (eOpenCloseType)ownerElement.TypeCode;
                var CurrentLayerName = eOpenClose.GetDataInfo().LayerName;

                if (!_layerService.Contains(CurrentLayerName))
                    _layerService.Add(CurrentLayerName);

                PromptPointResult pr = Doc.Editor.GetPoint(new PromptPointOptions("\nFirst corner:"));
                if (pr.Status != PromptStatus.OK) return null;
                basePoint = pr.Value;

                // Draw Body
                var bodyDisplayItems = MakeBody(eBodyType.Rectangle, ownerElement, basePoint);
                if (bodyDisplayItems.Any())
                {
                    displayItems.AddRange(bodyDisplayItems);
                }

                if (displayItems.Any())
                {
                    ObjectId ownerObjectId = displayItems.First().ObjectId;
                    ownerObjectId.XOpenForWrite(ent => ent.XAddData(ownerElement));
                }
            }
            catch (System.Exception ex)
            {
                string message = $"{PluginSettings.Prompt}Draw closure error: {ex}, {ex.Source}";
                Doc.Editor.WriteMessage(message);
                Plugin.Logger.Error($"{nameof(MakeClosureConnect)} error: ", ex);
            }
            finally
            {
                Selects.Clean();
            }

            return displayItems;
        }

        #endregion <ClosureConnect>

        #region <Cabinet>

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.Cabinet, CommandFlags.Session)]
        public void CabinetOpen()
        {
            //var commandParams = new CommandParams() { ParamAction = "New", ParamTypeId = eClosureType.CL };
            var commandArgs = new CommandArgs(null, CommandNames.Cabinet, new AcadCabinet(eCabinetType.AGC));
            ToolsManager.LoadPallete(PaletteNames.Cabinet, commandArgs);
        }

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddCabinet, CommandFlags.Session)]
        public void CabinetAdd()
        {
            var panelContext = Plugin.GetService<ICabinetPanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.BodyElementDataContext.CurrentElement;

            //Db.InsertBlock("closure", Point3d.Origin, new BlockOptions { Scale = 1, JigPrompt = eJigPrompt.PromptInsert });
            var objIdItems = MakeCabinet(ownerElement);
            if (objIdItems != null)
            {
                panelContext.IsLoaded = false;
                panelContext.ElementItems.Clear();
                panelContext.ElementItems.AddRange(objIdItems);
                panelContext.IsLoaded = true;
            }
        }

        [CommandAspect]
        [CommandMethod(CommandNames.MainGroup, CommandNames.UpdateCabinet, CommandFlags.Session | CommandFlags.NoHistory)]
        public void CabinetUpdate()
        {
            var panelContext = Plugin.GetService<ICabinetPanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.BodyElementDataContext.CurrentElement;
            ownerElement.ObjectState = ObjectState.Edit;

            var objIdItems = MakeCabinet(ownerElement).ToList();
            if (objIdItems != null)
            {
                //AcadCabinet element = Db.XGetXDataObject(objIdItems.First().ObjectId.Handle.ToString()) as AcadCabinet;
                //if (element != null)
                //    ownerElement = element;
                panelContext.ElementItems.Clear();
                panelContext.ElementItems.AddRange(objIdItems);
            }

            ownerElement.ObjectState = ObjectState.Unchanged;
        }

        [CommandAspect]
        [CommandMethod(CommandNames.UserGroup, CommandNames.AddTitleCabinet, CommandFlags.Session)]
        public void CabinetAddTitle()
        {
            var panelContext = Plugin.GetService<ICabinetPanelContext>();
            if (panelContext == null) return;

            var ownerElement = panelContext.BodyElementDataContext.CurrentElement;

            DBObject entityOwner = Db.XGetObject(ownerElement.Handle);
            var elementOwner = entityOwner.XGetDataObject();
            if (entityOwner == null || elementOwner == null) return;

            elementOwner.Title = ownerElement.Title;

            IEnumerable<ObjectIdItem> titleItems = MakeTitle(elementOwner, ((Entity)entityOwner).XGetMidPoint());
            if (titleItems != null)
                panelContext.ElementItems.AddRange(titleItems);

            entityOwner.ObjectId.XOpenForWrite(ent => ent.XAddData(elementOwner));
        }

        void CabinetUpgradeDbObject(DBObject dbObject, IPaletteElement elementPrototype, ObjectState objectState = ObjectState.Unchanged)
        {
            var currentLayerName = elementPrototype.LayerName;

            if (!_layerService.Contains(currentLayerName))
                _layerService.Add(currentLayerName);

            using (Db.TransactionManager.StartTransaction())
            {
                IPaletteElement element;
                if (dbObject.GetType() == typeof(DBText))
                {
                    DBText entity = (DBText)Db.XGetObject(dbObject.Handle.ToString());
                    entity.UpgradeOpen();
                    entity.TextString = elementPrototype.Title;
                    entity.Layer = elementPrototype.LayerName;

                    element = dbObject.XGetXDataObject<AcadCabinet>();
                    element.Update(elementPrototype, objectState);
                    element.Title = elementPrototype.Title;

                    dbObject.XAddData(element);
                    entity.DowngradeOpen();
                }
                if (dbObject.GetType() == typeof(Polyline))
                {
                    element = dbObject.XGetXDataObject<AcadCabinet>();
                    element.Update(elementPrototype, objectState);
                    dbObject.XAddData(element);
                }
                else
                {
                    return;
                }

                if (element.Items.Length > 0)
                {
                    foreach (var itemHandle in element.Items)
                    {
                        dbObject = Db.XGetObject(itemHandle);
                        CabinetUpgradeDbObject(dbObject, elementPrototype, objectState);
                    }
                }
            }
        }

        /// <summary> Cabinet </summary>
        public IEnumerable<ObjectIdItem> MakeCabinet(AcadCabinet acadCabinet)
        {
            Edit.IsRemakeMode = acadCabinet.Handle != "";
            acadApp.SetSystemVariable("SNAPMODE", 1);
            List<ObjectIdItem> displayItems = new List<ObjectIdItem>();

            var draw = Plugin.GetService<IDrawService>();
            try
            {
                Entity ownerEntity = Cabinet(Point3d.Origin.X, Point3d.Origin.Y, acadCabinet,
                    Color.FromColor(System.Drawing.Color.Black).ColorIndex);

                Point3d basePoint;
                if (Edit.IsRemakeMode)
                {
                    Polyline currentOwnerObject = (Polyline)Db.XGetObject(acadCabinet.Handle);
                    basePoint = currentOwnerObject.XGetMidPoint();
                    Db.XRemoveObjects(acadCabinet, typeof(DBText));
                }
                else
                {
                    ElementEntityJig jig = new ElementEntityJig(ownerEntity, acadCabinet);
                    PromptResult promptResult = jig.Drag(eDragType.Location);
                    basePoint = jig.BasePoint;

                    if (promptResult.Status == PromptStatus.Cancel | promptResult.Status == PromptStatus.Error)
                        return null;
                }

                ObjectId ownerObjectId;
                using (Db.TransactionManager.StartTransaction())
                {
                    if (acadCabinet.ColorIndex.HasValue)
                        ownerEntity.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)acadCabinet.ColorIndex);

                    var currentLayerName = acadCabinet.TypeCode.GetDataInfo().LayerName;

                    if (!_layerService.Contains(currentLayerName))
                        _layerService.Add(currentLayerName);

                    ownerEntity.Layer = currentLayerName;

                    if (Edit.IsRemakeMode && acadCabinet.Height.HasValue)
                        ownerEntity.TransformBy(
                            Matrix3d.Displacement(basePoint.GetAsVector() +
                                                  new Vector3d(0, (short)acadCabinet.Height / 2, 0)));

                    ownerEntity.XSaveChanges();
                    ownerObjectId = ownerEntity.ObjectId;

                    acadCabinet.BasePoint = basePoint;
                    acadCabinet.Handle = ownerEntity.Handle.ToString();
                    acadCabinet.OwnerHandle = ownerEntity.Handle.ToString();

                    if (acadCabinet.TypeCode == eCabinetType.HFD)
                    {
                        Entity donut = DrawObject.Donut(basePoint.X, basePoint.Y, 0, 3, 255);
                        donut.XSaveChanges();
                        acadCabinet.Items = acadCabinet.Items.Concat(new[] { donut.Handle.ToString() }).ToArray();
                    }
                }

                List<ObjectIdItem> titleDisplayItems = new List<ObjectIdItem>();
                if (!Edit.IsRemakeMode)
                {
                    titleDisplayItems = MakeTitle(acadCabinet, basePoint).ToList();
                }
                else
                {
                    var db = ownerEntity.Database;
                    using (ownerEntity.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var objectId in acadCabinet.XGetObjectIdItems(typeof(DBText)))
                        {
                            DBText dbObject = (DBText)db.XGetObject(objectId);

                            dbObject.UpgradeOpen();
                            dbObject.TextString = acadCabinet.Title;
                            dbObject.Layer = acadCabinet.LayerName;
                            //dbObject.TextString.Replace(acadCabinet.TypeCode == eCabinetType.FHD ? "AGC" : "FHD", acadCabinet.TypeCode == eCabinetType.FHD ? "FHD" : "AGC");
                            AcadTitle element = (AcadTitle)dbObject.XGetXDataObject(typeof(AcadTitle));
                            element.Update(acadCabinet.TypeCode);
                            element.Title = dbObject.TextString;
                            element.OwnerHandle = acadCabinet.Handle;

                            dbObject.XUpgradeOpen(element);
                            dbObject.DowngradeOpen();

                            titleDisplayItems.Add(element.GetListItem(dbObject));
                        }
                    }
                }

                ownerObjectId.XOpenForWrite(ent =>
                {
                    ent.XAddData(acadCabinet);
                    ObjectIdItem item = ent.XGetDisplayItem(acadCabinet);
                    displayItems.Add(item);
                });

                if (titleDisplayItems.Any())
                    displayItems.AddRange(titleDisplayItems);
            }
            catch (System.Exception ex)
            {
                string message = $"{PluginSettings.Prompt}Draw closure error: {ex}, {ex.Source}";
                Doc.Editor.WriteMessage(message);
                Plugin.Logger.Error($"{nameof(DrawService)}.{nameof(MakeCabinet)} error: ", ex);
            }
            finally
            {
                Edit.IsRemakeMode = false;
                Selects.Clean();
            }

            return displayItems;
        }

        /// <summary> Donut </summary>
        private Entity Cabinet(double pntX, double pntY, AcadCabinet cadCabinet, short colorIndex = -1)
        {
            Point3d p1 = Point3d.Origin;
            Point3d p2 = Point3d.Origin + new Vector3d(0, 0, 0);

            if (!cadCabinet.Height.HasValue || !cadCabinet.Width.HasValue)
            {
                Plugin.Logger.Error("cadCabinet.Height and cadCabinet.Width must contains value!");
                return null;
            }

            //Doc.Editor.DrawVector(p1, p2, 1, true);

            double h = (double)cadCabinet.Height; //p1.DistanceTo(p2);
            double wid = (double)cadCabinet.Width;

            Doc.Editor.WriteMessage("\n\tLength:\t{0:f3}\tWidth:{1:f3}\n", h, wid);

            Plane plan = new Plane(Point3d.Origin, Vector3d.ZAxis);
            double ang = p1.GetVectorTo(p2).AngleOnPlane(plan);
            Point3dCollection pts = new Point3dCollection();

            Point3d c1 = p1.XGetPolarPoint(ang - Math.PI / 2, wid / 2);
            Point3d c4 = p1.XGetPolarPoint(ang + Math.PI / 2, wid / 2);
            Point3d c2 = c1.XGetPolarPoint(ang, h);
            Point3d c3 = c4.XGetPolarPoint(ang, h);

            pts.Add(c1);
            pts.Add(c2);
            pts.Add(c3);
            pts.Add(c4);

            Polyline pline = new Polyline();
            if (cadCabinet.TypeCode == eCabinetType.AGC)
            {
                Point3d c5 = c1;
                Point3d c6 = c3;
                Point3d c7 = c2;
                Point3d c8 = c4;
                pts.Add(c5);
                pts.Add(c6);
                pts.Add(c7);
                pts.Add(c8);
            }
            else
            {
                pline.Closed = true;
            }

            int idx = 0;
            foreach (Point3d p in pts)
            {
                pline.AddVertexAt(idx, p.XGetPoint2d(), 0, (double)cadCabinet.Weight, (double)cadCabinet.Weight);
                idx += 1;
            }

            pline.SetDatabaseDefaults();

            return pline;
        }

        #endregion

        #region <Make elements>

        [CommandAspect]
        /// <summary> Title text </summary>
        public IEnumerable<ObjectIdItem> MakeTitle<T>(T ownerElement, Point3d basePoint, int? titleCount = null)
            where T : IPaletteElement
        {
            List<ObjectIdItem> titleDisplayItems = new List<ObjectIdItem>();
            DBText textPrototype = null;

            int currentTitleCount = 0;
            do
            {
                currentTitleCount = currentTitleCount + 1;
                PromptStatus promptStatus;

                DBText titleText = textPrototype ?? new DBText
                {
                    TextString = ownerElement.Title,
                    Position = basePoint,
                    HorizontalMode = (TextHorizontalMode)ownerElement.TextAlign,
                    Layer = ownerElement.LayerName,
                    ColorIndex = ownerElement.TitleColorIndex.HasValue ? (int)ownerElement.TitleColorIndex : 0,
                    Height = !ownerElement.Height.HasValue || (short)ownerElement.Height == 0 ? 1 : (short)ownerElement.Height,
                    LineWeight = (LineWeight)ownerElement.Weight
                };

                TextPlacementJig jig = new TextPlacementJig(Db, titleText, false);
                promptStatus = jig.Drag();

                if (promptStatus != PromptStatus.OK && ownerElement.Items.Length >= 1)
                {
                    break;
                }

                if (promptStatus != PromptStatus.OK)
                    titleText.Position = basePoint;
                else
                {
                    var jigEntity = jig.GetEntity();
                    titleText.Position = jigEntity.Position;
                    if (jigEntity.HorizontalMode != TextHorizontalMode.TextLeft)
                    {
                        titleText.AlignmentPoint = jigEntity.Position;
                    }
                    titleText.Rotation = jigEntity.Rotation;
                    titleText.Height = jigEntity.Height;
                }

                titleText.AddToCurrentSpace();

                PaletteElement elementTitle = new PaletteElement()
                {
                    Title = ownerElement.Title,
                    OwnerHandle = ownerElement.OwnerHandle,
                    OwnerFullType = ownerElement.OwnerFullType,
                    ParentHandle = ownerElement.OwnerHandle,
                    Handle = titleText.Handle.ToString(),
                    ElementName = typeof(Title).Name,
                    ElementType = typeof(Title).FullName,
                    TypeCodeFullName = typeof(eTitleType).FullName,
                    TypeCode = (int?)eTitleType.Default,
                    PaletteType = ownerElement.PaletteType
                };

                titleText.ObjectId.XOpenForWrite(ent => titleText.XAddData(elementTitle));

                ownerElement.Items = ownerElement.Items.Concat(new[] { titleText.Handle.ToString() }).ToArray();
                var titleResult = titleText.XGetDisplayItem(elementTitle);

                if (titleResult != null)
                    titleDisplayItems.Add(titleResult);

                if (titleResult == null
                    || promptStatus != PromptStatus.OK
                    || ownerElement.GetType() == typeof(T)
                    || currentTitleCount == titleCount)
                {
                    break;
                }

                textPrototype = titleText.XGetPrototype();

            } while (true);

            TextPlacementJig.Clear();

            return titleDisplayItems;
        }

        [CommandAspect]
        public IEnumerable<ObjectIdItem> MakeBody(eBodyType eBodyType, IPaletteElement ownerElement, Point3d basePoint)
        {
            Entity body = null;
            // Draw rectangle
            List<ObjectIdItem> displayItems = new List<ObjectIdItem>();

            switch (eBodyType)
            {
                case eBodyType.Title:
                    break;
                case eBodyType.Rectangle:
                    if (basePoint == Point3d.Origin)
                    {
                        PromptPointOptions prOpt = new PromptPointOptions("\nFirst corner:");
                        PromptPointResult pr = Doc.Editor.GetPoint(prOpt);
                        if (pr.Status != PromptStatus.OK) return null;
                        basePoint = pr.Value;
                    }

                    RectangleDrawJig jigger = new RectangleDrawJig(basePoint, "\nSecond corner:");
                    PromptResult rectangPromptResult = Doc.Editor.Drag(jigger);
                    if (rectangPromptResult.Status != PromptStatus.OK) return null;

                    body = DrawObject.Pline(jigger.Corners.OfType<Point3d>().ToArray());
                    ((Polyline)body).Closed = true;
                    break;
                case eBodyType.Polyline:
                    break;
                case eBodyType.Donut:
                    break;
                case eBodyType.Circle:
                    break;
                case eBodyType.Blockreference:
                    break;
                default:
                    return null;
            }

            //var colorIndex = Colors.GetColorFromIndex((short)ownerElement.ColorIndex);
            var currentLayerName = eBodyType.GetAttribute<DataInfoAttribute>().LayerName;

            if (!_layerService.Contains(currentLayerName))
                _layerService.Add(currentLayerName);

            var typedLayer = _layerService.Get(currentLayerName);

            body.Layer = typedLayer.LayerName;
            body.ColorIndex = (int)ownerElement.ColorIndex == -1
                ? typedLayer.ColorIndex : (int)ownerElement.ColorIndex;

            var ownerObjectId = body.AddToCurrentSpace();
            basePoint = body.XGetMidPoint();

            ownerObjectId.XOpenForWrite(ent =>
            {
                ownerElement.Handle = ent.Handle.ToString();
                ownerElement.BodyType = (int)eBodyType;
                ownerElement.OwnerHandle = ent.Handle.ToString();
                ownerElement.OwnerFullType = typeof(AcadClosureConnect).FullName;
                ownerElement.ElementType = ownerElement.OwnerFullType;
                ownerElement.ElementName = typeof(AcadClosureConnect).Name;
                ownerElement.Weight = (short)LineWeight.LineWeight013;
                ownerElement.PaletteType = (short)PaletteNames.ClosureConnect;
                ent.XAddData(ownerElement);

                ObjectIdItem item = ent.XGetDisplayItem(ownerElement);
                displayItems.Add(item);
            });

            return displayItems;
        }

        [CommandAspect]
        /// <summary> Marker( </summary>
        public IEnumerable<ObjectIdItem> MakeMarker(IPaletteElement markerElement, IPaletteElement ownerElement, Point3d basePoint)
        {
            List<ObjectIdItem> displayItems = new List<ObjectIdItem>();

            ElementEntityJig jig = new ElementEntityJig(new Circle(basePoint, Vector3d.ZAxis, 50), ownerElement);
            PromptResult promptResult = jig.Drag(eDragType.Location);
            basePoint = jig.BasePoint;

            if (promptResult.Status == PromptStatus.Cancel || promptResult.Status == PromptStatus.Error)
                return null;

            Entity donut = DrawObject.Donut(basePoint.X, basePoint.Y, 0, 50, (short)ownerElement.ColorIndex, 50);
            donut.AddToCurrentSpace();

            donut.ObjectId.XOpenForWrite<Entity>(ent =>
            {
                ent.Layer = ownerElement.LayerName;

                PaletteElement element = new PaletteElement
                {
                    Handle = ent.Handle.ToString(),
                    Title = "Marker",
                    BodyType = (int)eBodyType.Marker,
                    TypeCode = (int)markerElement.TypeCode,
                    TypeCodeFullName = typeof(eOpenCloseType).FullName,
                    LayerName = "BUILDING PARTNER",
                    OwnerHandle = ownerElement.Handle,
                    OwnerFullType = ownerElement.GetType().FullName,
                    ParentHandle = ownerElement.Handle,
                    PaletteType = ownerElement.PaletteType
                };
                ent.XAddData(element);

                ownerElement.Items = ownerElement.Items.Concat(new[] { ent.Handle.ToString() }).ToArray();
                ObjectIdItem item = ent.XGetDisplayItem(element);
                displayItems.Add(item);
            });

            return displayItems;
        }

        #endregion <Make elements>
    }
}