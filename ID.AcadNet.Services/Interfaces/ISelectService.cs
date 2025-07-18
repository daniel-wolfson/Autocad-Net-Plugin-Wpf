using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface ISelectService
    {
        void HighlightSubEntity(Document doc, PromptNestedEntityResult rs);

        [Autodesk.AutoCAD.Runtime.CommandMethod("SubEntSelect")]
        void SubEntSelect();

        [Autodesk.AutoCAD.Runtime.CommandMethod("SelectNested")]
        void SelectNested();

        void GetBlockNameFromItsSubentity();
        SelectionSet GetSelectionSet();
        List<Point3d> GetPromptPoints(int tPromptCount, OptionsGetPoints tOption);
        void SelectClear();
        List<ObjectId> GetSelect();

        /// <summary> Get implied objects </summary>
        List<ObjectId> GetSelectImplied(List<ObjectId> objectIds = null);

        /// <summary> Get implied objects </summary>
        List<ObjectId> GetImplied();

        List<Entity> GetEntitiesOld(OptionsGetSelect tOptionPolygon, Point3dCollection tPoint3DColl,
            params string[] tSelectFilter);

        Entity GetEntity(OptionsGetSelect tOption = OptionsGetSelect.GetObject, string tPrompt = "",
            Point3dCollection tPolygonPoint3DColl = null, params string[] tEntityFilter);

        List<Entity> GetEntities(OptionsGetSelect tOption, string tPrompt = "", Point3dCollection tPolygonPoint3DColl = null,
            params string[] tEntityFilter);

        void SelectBlockByName(string blockName);
        List<ObjectId> SelectBlocks(string blockNamePattern = "");
    }
}