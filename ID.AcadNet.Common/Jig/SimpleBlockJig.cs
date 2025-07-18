using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class SimpleBlockJig : DrawJig
    {
        public Point3d _point;
        private ObjectId _blockId = ObjectId.Null;

        // Shows the block until the user clicks a point.
        // The 1st parameter is the Id of the block definition.
        // The 2nd is the clicked point.
        //---------------------------------------------------------------
        public PromptResult DragMe(ObjectId i_blockId, out Point3d o_pnt)
        {
            _blockId = i_blockId;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptResult jigRes = ed.Drag(this);
            o_pnt = _point;
            return jigRes;
        }


        // Need to override this method.
        // Updating the current position of the block.
        //--------------------------------------------------------------
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jigOpts = new JigPromptPointOptions();
            jigOpts.UserInputControls =
                (UserInputControls.Accept3dCoordinates |
                 UserInputControls.NullResponseAccepted);
            jigOpts.Message = "Select a point:";
            PromptPointResult jigRes = prompts.AcquirePoint(jigOpts);

            Point3d pt = jigRes.Value;
            if (pt == _point)
                return SamplerStatus.NoChange;

            _point = pt;
            if (jigRes.Status == PromptStatus.OK)
                return SamplerStatus.OK;

            return SamplerStatus.Cancel;
        }


        // Need to override this method.
        // We are showing our block in its current position here.
        //--------------------------------------------------------------
        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            BlockReference inMemoryBlockInsert =
                new BlockReference(_point, _blockId);
            draw.Geometry.Draw(inMemoryBlockInsert);

            inMemoryBlockInsert.Dispose();

            return true;
        } // WorldDraw()

    }
}