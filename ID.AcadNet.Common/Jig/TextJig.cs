using System;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class TextJig : DrawJig
    {
        public Point3d Position { get; set; }

        // We'll keep our style alive rather than recreating it
        private readonly TextStyle _style;
        public TextJig()
        {
            _style = new TextStyle
            {
                Font = new FontDescriptor("Calibri", false, true, 0, 0),
                TextSize = 10
            };
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var opts = new JigPromptPointOptions { UserInputControls = UserInputControls.Accept3dCoordinates, Message = "\nSelect point: " };
            var res = prompts.AcquirePoint(opts);
            if (res.Status == PromptStatus.OK)
            {
                if (Position == res.Value)
                {
                    return SamplerStatus.NoChange;
                }
                Position = res.Value;
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            if (draw == null) throw new ArgumentNullException("draw");
            // We make use of another interface to push our transforms
            var wg2 = draw.Geometry;
            if (wg2 != null)
            {
                // Push our transforms onto the stack
                wg2.PushOrientationTransform(OrientationBehavior.Screen);
                wg2.PushPositionTransform(PositionBehavior.Screen, new Point2d(30, 30));
                // Draw our screen-fixed text
                // Position
                // Normal
                // Direction
                // Text
                // Rawness
                // TextStyle
                wg2.Text(new Point3d(0, 0, 0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), Position.ToString(), true, _style);
                // Remember to pop our transforms off the stack
                wg2.PopModelTransform();
                wg2.PopModelTransform();
            }
            return true;
        }

        //protected override bool WorldDraw(WorldDraw draw)
        //{
        //    throw new NotImplementedException();
        //}
    }
}