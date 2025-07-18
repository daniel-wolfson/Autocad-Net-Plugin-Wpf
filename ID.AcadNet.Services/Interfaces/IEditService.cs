using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface IEditService
    {
        double RotateCnt(int tCntId = -1, OptionsGetSelect tOption = OptionsGetSelect.SelectLast);
        void XAddDataToNOD(string tNODDataName = "");
        void UpdateBlockAttributeByName(string tBlockName, string tAttrName, string tNewValue);
        ObjectId GetObjectId(string hnd);
        Entity GetObject(string hnd);
        void RemoveObject(string hnd);
        void RemoveObject(ObjectId objId);
        void RemoveObjects(string[] tLayerObject);
        void EraseObject();
        Entity GetObjectByHandle(string handle);
        Entity GetObjectByHandle(long handle);
        ObjectId GetObjectIdByHandle(string handle);
        object GetObjXData(int tCntId);

        ObjectIdCollection GetObjXData(OptionsGetSelect tPromptOption = OptionsGetSelect.All, int tCntId = -1,
            OptionsGetResult tReturnOption = OptionsGetResult.ObjectIdColl, string tXDataParameter = "CntId");

        void XSetDataByObjectId(ObjectId tObjId, params string[] tParams);
        void UpdateXData(ObjectId tObjId, TypeCode tTypeCode, int tValue);
        void AddRegAppTableRecord(string regAppName);
        void SetStoredRotation(DBObject obj, double rotation);
        double GetStoredRotation(DBObject obj);
        void ExplodeBlock(string blockName);
    }
}