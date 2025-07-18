//
// (C) Copyright 2004-2009 by Autodesk, Inc.
//
//
//
// By using this code, you are agreeing to the terms
// and conditions of the License Agreement that appeared
// and was accepted upon download or installation
// (or in connection with the download or installation)
// of the Autodesk software in which this code is included.
// All permissions on use of this code are as set forth
// in such License Agreement provided that the above copyright
// notice appears in all authorized copies and that both that
// copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

// ImportExportCS.CS

using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Gis.Map;
using Autodesk.Gis.Map.ImportExport;
using Autodesk.Gis.Map.ObjectData;
using Autodesk.Gis.Map.Project;

namespace Intellidesk.AcadNet.Common.Utils
{
    /// <summary>
    /// Event handlers for export.
    /// </summary>
    public sealed class MyExpEventHandler
    {
        public MyExpEventHandler(StreamWriter sw)
        {
            m_out = sw;
        }

        public void RecordReadyForExport(object sender, RecordReadyForExportEventArgs args)
        {
            args.ContinueExport = true;
        }

        public void RecordExported(object sender, RecordExportedEventArgs args)
        {
        }

        public void RecordError(object sender, ExportRecordErrorEventArgs args)
        {
            string errorMsg = args.Error;
            if ((null != errorMsg) && (errorMsg.Length == 0))
            {
                string outMsg = "\nEntity";
                m_out.WriteLine(outMsg);
            }
        }

        private StreamWriter m_out;
    }

    /// <summary>
    /// Event handlers for import.
    /// </summary>
    public sealed class MyImpEventHandler
    {
        public MyImpEventHandler()
        {
            m_importedIds = new ObjectIdCollection();
        }

        private ObjectIdCollection m_importedIds;
        public ObjectIdCollection ImportedEntities
        {
            get
            {
                return m_importedIds;
            }
        }

        public void RecordReadyForImport(Object sender, RecordReadyForImportEventArgs args)
        {
            args.ContinueImport = true;
        }

        public void RecordImported(Object sender, RecordImportedEventArgs args)
        {
            m_importedIds.Add(args.ObjectId);
        }

        public void RecordError(Object sender, ImportRecordErrorEventArgs args)
        {
        }
    }

    public sealed class SampleCommands
    {

        #region " Singleton "
        private static SampleCommands m_Instance = new SampleCommands();

        private SampleCommands()
        {
        }
        public static SampleCommands Instance
        {
            get
            {
                return m_Instance;
            }
        }
        #endregion

        private MyExpEventHandler m_ExpEventHandler;
        private RecordReadyForExportEventHandler m_ExpRecordReadyHandler;
        private RecordExportedEventHandler m_ExpRecordExportedHandler;
        private ExportRecordErrorEventHandler m_ExpRecordErrorHandler;

        private MyImpEventHandler m_ImpEventHandler;
        private RecordReadyForImportEventHandler m_ImpRecordReadyHandler;
        private RecordImportedEventHandler m_ImpRecordImportedHandler;
        private ImportRecordErrorEventHandler m_ImpRecordErrorHandler;
        public MyImpEventHandler ImportEventManager
        {
            get
            {
                return m_ImpEventHandler;
            }
        }

        /// <summary>
        /// List the Commands this sample provided.
        /// </summary>
        [CommandMethod("CmdList")]
        public static void CmdList()
        {
            Utility.ShowMsg("\n ImportExport sample commands : \n");
            Utility.ShowMsg(" Cmd : RunImport \n");
            Utility.ShowMsg(" Cmd : RunExport \n");
        }

        [CommandMethod("RunImport")]
        public static void RunImport() 
        {
            //FormImport formImport = new FormImport();
            //formImport.ShowDialog();
        }

        [CommandMethod("RunExport")]
        public static void RunExport() 
        {
            //FormExport formExport = new FormExport();
            //formExport.ShowDialog();
        }

        /// <summary>
        /// Exports all of the entities or exports entities layer by layer.
        /// </summary>
        public void DoExport(string format, string expFile, string layerFilter, 
            string isLogFile, bool isODTable, bool isLinkTemplate)
        {
            string msg = null;

            FileStream fs = new FileStream(isLogFile, FileMode.Append);
            StreamWriter log = new StreamWriter(fs);

            Exporter exporter = null;

            try
            {
                // Get current time and log the time of executing exporting.
                DateTime time = DateTime.UtcNow;
                log.WriteLine(time.ToLocalTime().ToString());
                log.Write("Exporting file ");
                log.WriteLine(expFile);

                MapApplication mapApp = HostMapApplicationServices.Application;
                exporter = mapApp.Exporter; 

                // Initiate the exporter
                exporter.Init(format, expFile);

                // Create event handlers 
                if (null == m_ExpEventHandler)
                {
                    m_ExpEventHandler = new MyExpEventHandler(log);
                    m_ExpRecordReadyHandler = new RecordReadyForExportEventHandler(m_ExpEventHandler.RecordReadyForExport);
                    m_ExpRecordExportedHandler = new RecordExportedEventHandler(m_ExpEventHandler.RecordExported);
                    m_ExpRecordErrorHandler = new ExportRecordErrorEventHandler(m_ExpEventHandler.RecordError);
                }

                // Attach Event Handlers to the Exporter
                exporter.RecordReadyForExport += m_ExpRecordReadyHandler;
                exporter.RecordExported += m_ExpRecordExportedHandler;
                exporter.ExportRecordError += m_ExpRecordErrorHandler;

                // Get Data mapping object
                ExpressionTargetCollection dataMapping = null;
                dataMapping = exporter.GetExportDataMappings();

                // Set ObjectData data mapping if isODTable is true
                if (isODTable && !MapODData(dataMapping))
                {
                    log.WriteLine("Error in mapping OD table data!");
                }

                // Reset Data mapping with Object data and Link template keys        
                exporter.SetExportDataMappings(dataMapping);

                // If layerFilter isn't null, set the layer filter to export layer by layer
                if (null != layerFilter)
                {
                    exporter.LayerFilter = layerFilter;
                }

                // Do the exporting and log the result
                ExportResults results;
                results = exporter.Export(true);
                msg = string.Format("    {0} objects are exported.", results.EntitiesExported.ToString());
                log.WriteLine(msg);
                msg = string.Format("    {0} objects are skipped.", results.EntitiesSkippedCouldNotTransform.ToString());
                log.WriteLine(msg);

                Utility.ShowMsg("\nExporting succeeded.");
            }
            catch (MapException e) 
            {
                log.WriteLine(e.Message);
                log.WriteLine(e.ErrorCode.ToString());
                Utility.ShowMsg("\nExporting failed.");
            }
            finally 
            {
                log.WriteLine(); 
                log.Close();

                // Remove Event Handlers
                if (null != m_ExpEventHandler)
                {
                    exporter.RecordReadyForExport -= m_ExpRecordReadyHandler;
                    exporter.RecordExported -= m_ExpRecordExportedHandler;
                    exporter.ExportRecordError -= m_ExpRecordErrorHandler;
                }
            }
        }

        /// <summary>
        /// Gets all of the layers names in the current drawing
        /// </summary>
        /// <param name="layerNames">[in] Storage of the layer names, cannot be null.</param>
        /// <returns>Returns true if successful.</returns>
        /// <remarks>Throws no exceptions in common condition.</remarks>
        public bool GetLayers(StringCollection layerNames)
        {
            // Get the active database
            Database db = HostApplicationServices.WorkingDatabase;

            bool success = true;
            using (Transaction trans = CurrentTransactionManager.StartTransaction())
            {
                LayerTable layerTable = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (null == layerTable)
                    return false;
    
                foreach (ObjectId id in layerTable)
                {
                    LayerTableRecord layerTableRecord = null;
                    
                    layerTableRecord = trans.GetObject(id, OpenMode.ForRead) as LayerTableRecord;
                    if (null == layerTableRecord)
                    {
                        success = false;
                        continue;
                    }
                    
                    // Get the layer name
                    layerNames.Add(layerTableRecord.Name);
                }
            }

            return success;
        }

        /// <summary>
        /// Map ObjectData to attribute fields in the exported-to file.
        /// </summary>
        public bool MapODData(ExpressionTargetCollection mapping)
        {
            MapApplication mapApi = null; 
            ProjectModel proj = null; 
            Tables tables = null; 
            Autodesk.Gis.Map.ObjectData.Table table = null; 
            StringCollection tableNames = null;
            FieldDefinitions definitions = null;

            // Get map session and all the OD tables
            MapApplication mapApp = HostMapApplicationServices.Application;
            mapApi = mapApp; 
            proj = mapApi.ActiveProject;
            tables = proj.ODTables;
            tableNames = tables.GetTableNames();

            // Iterate through the OD table definition and get all the field names
            int tableCount = tables.TablesCount;
            try
            {
                for (int i = 0; i < tableCount; i++)
                {
                    table = tables[tableNames[i]];
                    definitions = table.FieldDefinitions;
                    for (int j = 0; j < definitions.Count; j++)
                    {
                        FieldDefinition column = null;
                        column = definitions[j];
                        // fieldName is the OD table field name in the data mapping. It should be 
                        // in the format:fieldName&tableName. 
                        // newFieldName is the attribute field name of exported-to file
                        // It is set to ODFieldName_ODTableName in this sample
                        string newFieldName = null;
                        string fieldName = null;
                        fieldName = ":" + column.Name + "@" + tableNames[i];
                        newFieldName = column.Name + "_" + tableNames[i];
                        mapping.Add(fieldName, newFieldName);
                    }
                }
            }
            catch (MapImportExportException) 
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Executes importing.
        /// </summary>
        public bool ImportDwg(string format, string fileName, bool isToLayer0, bool isImpAttToOD)
        {
            // Init the importer with the format and name of import-from file
            MapApplication mapApp = HostMapApplicationServices.Application;
            Importer importer = mapApp.Importer;
            try
            {
                importer.Init(format, fileName);

                foreach (InputLayer layer in importer)
                {
                    Utility.ShowMsg("\nIn inputlayers for loop");
                    // Set name of the layer in MAP to import the entities to
                    // set the layer name as IMP_layerName. And change 
                    // the "." to "_" if there is any. Or set the layer name to 
                    // "0" if isToLayer0 is false.
                    string newLayerName = null;
                    if (!ChangeLayerName(layer, ref newLayerName, isToLayer0))
                    {
                        Utility.ShowMsg("\nFailed to set the layer name.");
                        continue;
                    }

                    if (isImpAttToOD)
                    {
                        // Set the table name as layername_OD
                        if (!SetTableName(layer, newLayerName))
                        {
                            Utility.ShowMsg("\nFailed to set the table name.");
                            continue;
                        }

                        // Set OD table field name from the attribute field name in 
                        // the imported-from file. 
                        // In this sample,it's set to sample_AttributeFieldName. 
                        foreach (Autodesk.Gis.Map.ImportExport.Column col in layer)
                        {
                            if (!SetColumnNames(col))
                            {
                                Utility.ShowMsg("\nFailed to set OD table field names");
                                continue;
                            }
                        }
                    } // End of if
                }

                // Attach event handlers
                if (null == m_ImpEventHandler)
                {
                    m_ImpEventHandler = new MyImpEventHandler();
                    m_ImpRecordReadyHandler = new RecordReadyForImportEventHandler(m_ImpEventHandler.RecordReadyForImport);
                    m_ImpRecordImportedHandler = new RecordImportedEventHandler(m_ImpEventHandler.RecordImported);
                    m_ImpRecordErrorHandler = new ImportRecordErrorEventHandler(m_ImpEventHandler.RecordError);
                }

                // Attach event handlers
                importer.RecordReadyForImport += m_ImpRecordReadyHandler;
                importer.RecordImported += m_ImpRecordImportedHandler;
                importer.ImportRecordError += m_ImpRecordErrorHandler;

                // Do importing and print out the result.
                ImportResults results;
                results = importer.Import(false);

                string msg = null;
                msg = string.Format("\n{0} entities are imported", results.EntitiesImported.ToString());
                Utility.ShowMsg(msg);
                msg = string.Format("\n{0} entities are skipped", results.EntitiesSkippedCouldNotTransform.ToString());
                Utility.ShowMsg(msg);
                msg = string.Format("\n{0} entities are with color close to the background", results.EntitiesWithColorCloseToBackground.ToString());
                Utility.ShowMsg(msg);

                return true;
            }
            catch (MapImportExportException)
            {
                return false;
            }
            finally
            {
                // Remove event handlers
                if (null != m_ImpEventHandler)
                {
                    importer.RecordReadyForImport -= m_ImpRecordReadyHandler;
                    importer.RecordImported -= m_ImpRecordImportedHandler;
                    importer.ImportRecordError -= m_ImpRecordErrorHandler;
                }
            }
        }

        /// <summary>
        /// Changes the import-to layer name to IMP_importFileName.
        /// If the importFileName is longer than  LayerNameLength, truncate it to LayerNameLength.
        /// If the importFileName contain characters other than alphabet, number, replace it with '_'
        /// </summary>
        /// <returns>Returns true if succeed.</returns>
        public bool ChangeLayerName(InputLayer layer, ref string layerName, bool isToLayer0)
        {
            LayerNameType layerNameType = LayerNameType.LayerNameDirect;

            try
            {
                if (isToLayer0)
                {
                    layerName = "0";
                }
                else
                {
                    string existingName = null;
                    // Truncate the layer name if it's too long
                    existingName = layer.Name;
                    // Replace illegal characters
                    foreach (Char tmp in existingName)
                    {
                        if (!(tmp >= 'A' && tmp <= 'Z'))
                        {
                            if (!(tmp >= 'a' && tmp <= 'z'))
                            {
                                if (!(tmp >='0' && tmp <='9'))
                                {
                                    existingName.Replace(tmp, '_');
                                }
                            }
                        }
                    }

                    // Prefix "IMP_"
                    layerName = string.Concat("IMP_", existingName);
                }

                layer.SetLayerName(layerNameType, layerName);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This function set the OD table name to layerName_OD.
        /// </summary>
        public bool SetTableName(InputLayer layer, string layerName)
        {
            ImportDataMapping tableType;
            tableType = ImportDataMapping.NewObjectDataOnly;
            string tableName = string.Concat(layerName, "_OD");
            string newTableName = null;
            // If the table name already exists, append a number after the table name,
            // until no OD table with the same name found.
            if (TableNameExist(tableName))
            {
                int index = 1;
                do
                {
                    newTableName = string.Concat(tableName, index.ToString());
                    index++;
                }
                while (TableNameExist(newTableName));

                layer.SetDataMapping(tableType, newTableName);
            }
            else
            {
                layer.SetDataMapping(tableType, tableName);
            }

            return true;
        }

        /// <summary>
        /// Check if an OD table name already exists in the dwg.
        /// </summary>
        public bool TableNameExist(string tableName)
        {
            try
            {
                // Get map session and all of the ObjectData tables
                MapApplication mapApp = HostMapApplicationServices.Application;
                ProjectModel proj = mapApp.ActiveProject;
                Tables tables = proj.ODTables;
                
                if (tables.IsTableDefined(tableName))
                    return true;

                return false;
            }
            catch (System.NullReferenceException)
            {
                return false;
            }
        }

        /// <summary>
        /// This function iterates all the attributes in import-from file
        /// and set the OD field name to be IMP_attributeName
        /// </summary>
        public bool SetColumnNames(Autodesk.Gis.Map.ImportExport.Column col)
        {
            string colName = col.ColumnName;
            string newFieldName = null;
            newFieldName = string.Concat("IESample_", colName);
            try
            {
                col.SetColumnDataMapping(newFieldName);
            }
            catch (MapImportExportException)
            {
                return false;
            }
            return true;
        }

        private Autodesk.AutoCAD.ApplicationServices.TransactionManager CurrentTransactionManager
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.TransactionManager;
            }
        }

        /// <summary>
        /// Changes the entities color to the selected display color.
        /// </summary>
        /// <param name="entityIdCollection">[in] The entity Ids to be high-lighted. </param>
        /// <param name="displayColor">[in] The display color index. </param>
        /// <param name="lockedEntityCount">[out] The locked entity count. </param>
        public void HighlightEntities(ObjectIdCollection entityIdCollection,
            int displayColor, ref long lockedEntityCount)
        {
            try
            {
                using (Transaction trans = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in entityIdCollection)
                    {
                        try
                        {
                            Entity entity = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                            
                            // Additional operations for BlockReference
                            // Should care about the entities in the block
                            if (entity is BlockReference)
                            {
                                BlockReference blkRef = entity as BlockReference;
                                
                                BlockTableRecord blkTblRecord = trans.GetObject(blkRef.BlockTableRecord,
                                    OpenMode.ForRead) as BlockTableRecord;
                                
                                foreach (ObjectId objId in blkTblRecord)
                                {
                                    // No catching here to simplify the logic
                                    Entity blkEntity = trans.GetObject(objId, OpenMode.ForWrite) as Entity;
                                    if (null != blkEntity)
                                        blkEntity.ColorIndex = displayColor;
                                }
                            }

                            // Set the color of the entity no matter if it is a BlockReference
                            entity.ColorIndex = displayColor;
                        }
                        catch (MapException e)
                        {
                            // locked entity found
                            if (e.ErrorCode == (int)ErrorStatus.OnLockedLayer)
                            {
                                lockedEntityCount++;
                            }
                        }
                    }

                    trans .Commit();
                }
            }
            catch(System.Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    } // End of Sample class

    public sealed class Utility
    {
        private Utility()
        {
        }

        public static void ShowMsg(string msg)
        {
            AcadEditor.WriteMessage(msg);
        }

        public static Autodesk.AutoCAD.EditorInput.Editor AcadEditor
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            }
        }
    }

    public sealed class ImportExportSampleApplication : IExtensionApplication
    {
        public void Initialize()
        {
            Utility.ShowMsg("\n ImportExportCS Sample Application initialized. \n");
        }

        public void Terminate()
        {
        }
    }
}
