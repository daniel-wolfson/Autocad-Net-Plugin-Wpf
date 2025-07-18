using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class InsertJig : EntityJig
    {
        public InsertJig(ObjectId blockId, Point3d position, Vector3d normal)
            : base(new BlockReference(position, blockId))
        {
            BlockReference.Normal = normal;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jigOpts = new JigPromptPointOptions
            {
                UserInputControls = UserInputControls.Accept3dCoordinates,
                Message = "" + "\nInsertion point: "
            };
            var res = prompts.AcquirePoint(jigOpts);
            var curPoint = res.Value;
            if (_position.DistanceTo(curPoint) > 0.0001)
            {
                _position = curPoint;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
            return res.Status == PromptStatus.Cancel ? SamplerStatus.Cancel : SamplerStatus.OK;
        }

        protected override bool Update()
        {
            try
            {
                if (BlockReference.Position.DistanceTo(_position) > 0.0001)
                {
                    BlockReference.Position = _position;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(InsertJig)}.{nameof(Update)} error: ", ex);
                Application.ShowAlertDialog(ex.Message);
            }
            return false;
        }

        public BlockReference BlockReference
        {
            get { return (BlockReference)Entity; }
        }

        private Point3d _position;
    }
}