using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface IOperationService
    {
        Editor Ed { get; }
        Database Db { get; }
        Document Doc { get; }
        Entity CurrentObject { get; set; }

        //event ToolsReadObjectsEventHandler ToolsReadObjectsEvent;
        //event ToolsParseBlockEventHandler ToolsParseBlockEvent;
        //event ToolsReadObjectsFilterEventHandler ToolsReadObjectsFilterEvent;
        //event ToolsReadObjectsEventHandler ToolsReadObjectsLineEvent;
        //event ToolsReadObjectsEventHandler ToolsReadObjectsPolylineEvent;
        //event ToolsReadObjectsEventHandler ToolsReadObjectsCircleEvent;
        //event ToolsReadObjectsEventHandler ToolsReadObjectsEllipseEvent;
        //event ToolsReadObjectsEventHandler ToolsReadObjectsMtextEvent;
        
        //void EntityFilter();
        //void MakeGroup();
        //void MakeXRecord();
        //void MakeXData();
        //double YMax(BlockTableRecord btr);
        //void AddMyDict2();
        //void InfoUsingLINQ();
        void OnWriteMessage(string message, params object[] parameters);
        string GetLineType(string lineTypeName);

        RegAppTableRecord RegApp { get; set; }
        ObjectId GetLineTypeObjectId(string value); //???

        string BrowserInvoke(string url, OptionsDocument tOptions);

        /// <summary> Operations with Documents </summary>
        bool DocumentAction(string tFileName, OptionsDocument tOptions);

        /// <summary> Find and Return Document </summary>
        Document DocumentFind(string tFullPathName);

        /// <summary> Dialog Open file </summary>
        string DocumenOpenDataDialog(string title, string tDataType, string dialogName);

        /// <summary> Prompt Open FileDialog </summary>
        string PromptOpenFileDialog(string tDataType);

        /// <summary> Dialog Save file </summary>
        void DocumenSaveDataDialog(string tDataType);

        /// <summary> On Documen Activated </summary>
        void OnDocumentActivated(object sender, DocumentCollectionEventArgs e);

        Dictionary<ObjectId, string> ReadObjectsDynamic(string dxfTypeNamesFilter, string layerNamesFilter, string fieldTextFilter);

        IEnumerable<ObjectId> ReadObjects(ActionArguments readArgs = null);
        DBObject GetObject(string typeNameFilter, string layerNameFilter, string fieldTextFilter);
        IEnumerable<ObjectId> GetObjects<T>(string layerNamesFilter, string fieldTextFilter) where T : DBObject;
        IEnumerable<ObjectId> GetObjects(string typeNameFilter, string layerNameFilter, string fieldTextFilter);
        IEnumerable<ObjectId> GetObjects(Type[] types, string layerNameFilter, string fieldTextFilter);

        void ReportProgress();
        bool LoadDataFromDwg(string tBlockName = "");
        void ReadDataFromXml(string tName = "");
        object LoadDataFromMdb(string tName = "", string tFilter = "", string tSort = "");
        string GetValueValid(string tVal, string tValDefault);
        void WriteDataToXml(string tName = "");
        void LoadDataFromXml(string fileName);
    }
}