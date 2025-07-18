using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;

namespace Intellidesk.AcadNet.Common.Models
{
    /// <summary> Options of create blocks </summary>
    public struct BlockOptions
    {
        public double Scale { get; set; }
        public eJigPrompt JigPrompt { get; set; }
        public bool IsImportBlocks { get; set; }
        public byte Transparency { get; set; }
        public string LayerName { get; set; }
        public Matrix3d Transform { get; set; }
        public bool IsRegen { get; set; }
        public bool IsRewrite { get; set; }
        public bool IsBlockExistCreateNew { get; set; }
        public IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();

        public BlockOptions(string layerName = null, double scale = 1.0, eJigPrompt jigPrompt = eJigPrompt.NoPrompt, byte transparency = 0, bool isImportBlocks = false) : this()
        {
            JigPrompt = jigPrompt;
            //InsertPoint = insertPoint ?? Point3d.Origin;
            var layerService = Plugin.GetService<ILayerService>();
            LayerName = layerService.GetWorkLayerName(layerName); //string workLayerName = context.LayerService.GetWorkLayerName("Temp_Intersect");
            Scale = scale;
            IsImportBlocks = isImportBlocks;
            Transparency = transparency;
            IsRegen = true;
        }
    }
}