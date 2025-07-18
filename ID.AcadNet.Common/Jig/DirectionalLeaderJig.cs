using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class DirectionalLeaderJig : EntityJig
    {
        private Point3d _start, _end;
        private string _contents;
        private int _index;
        private int _lineIndex;
        private bool _started;

        public DirectionalLeaderJig(string txt, Point3d start, MLeader ld) : base(ld)
        {
            // Store info that's passed in, but don't init the MLeader

            _contents = txt;
            _start = start;
            _end = start;

            _started = false;
        }

        // A fairly standard Sampler function

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var po = new JigPromptPointOptions();
            po.UserInputControls =
                (UserInputControls.Accept3dCoordinates |
                 UserInputControls.NoNegativeResponseAccepted);
            po.Message = "\nEnd point";

            var res = prompts.AcquirePoint(po);

            if (_end == res.Value)
            {
                return SamplerStatus.NoChange;
            }
            else if (res.Status == PromptStatus.OK)
            {
                _end = res.Value;
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            var ml = (MLeader)Entity;

            if (!_started)
            {
                if (_start.DistanceTo(_end) > Tolerance.Global.EqualPoint)
                {
                    // When the jig actually starts - and we have mouse movement -
                    // we create the MText and init the MLeader

                    ml.ContentType = ContentType.MTextContent;
                    var mt = new MText();
                    mt.Contents = _contents;
                    ml.MText = mt;

                    // Create the MLeader cluster and add a line to it

                    _index = ml.AddLeader();
                    _lineIndex = ml.AddLeaderLine(_index);

                    // Set the vertices on the line

                    ml.AddFirstVertex(_lineIndex, _start);
                    ml.AddLastVertex(_lineIndex, _end);

                    // Make sure we don't do this again

                    _started = true;
                }
            }
            else
            {
                // We only make the MLeader visible on the second time through
                // (this also helps avoid some strange geometry flicker)

                ml.Visible = true;

                // We already have a line, so just set its last vertex

                ml.SetLastVertex(_lineIndex, _end);
            }

            if (_started)
            {
                // Set the direction of the text to depend on the X of the end-point
                // (i.e. is if to the left or right of the start-point?)

                var dl = new Vector3d((_end.X >= _start.X ? 1 : -1), 0, 0);
                ml.SetDogleg(_index, dl);
            }

            return true;
        }
    }
}