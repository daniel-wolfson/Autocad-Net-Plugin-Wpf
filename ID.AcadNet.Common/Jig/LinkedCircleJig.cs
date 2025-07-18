using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class LinkedCircleJig : DrawJig
    {
        private Document _dwg;
        private Editor _ed;
        private Database _db;
        private ObjectId _circleId;
        private List<ObjectId> _fromLineIds;
        private List<ObjectId> _toLineIds;
        private Point3d _basePoint;
        private Point3d _currentPoint;
        private Point3d _prevPoint;
        private bool _cancelled = false;

        private Circle _visualCircle = null;
        private List<Line> _fromVisualLines = new List<Line>();
        private List<Line> _toVisualLines = new List<Line>();
        private int _visualColorIndex = 1;

        public LinkedCircleJig(Document dwg, ObjectId circleId)
        {
            _dwg = dwg;
            _ed = dwg.Editor;
            _db = dwg.Database;

            _circleId = circleId;
            //_basePoint = CommonUtil.GetCircleCentre(_db, circleId);
        }

        public void DragLinkedCircle()
        {
            _cancelled = false;

            try
            {
                CreateVisualEntities();
                _currentPoint = _basePoint;
                _prevPoint = _basePoint;
                HighlightEntities(true);

                _ed.Drag(this);

                if (!_cancelled)
                {
                    using (Transaction tran =
                        _db.TransactionManager.StartTransaction())
                    {
                        //Move circle
                        Vector3d displacement = _basePoint.GetVectorTo(_currentPoint);

                        Circle c = (Circle)tran.GetObject(
                            _circleId, OpenMode.ForWrite);
                        c.TransformBy(
                            Matrix3d.Displacement(displacement));

                        ObjectId fCircleId = new ObjectId();
                        ObjectId tCircleId = new ObjectId();

                        Point3d pt;
                        double dist;

                        //update FROM lines
                        foreach (var id in _fromLineIds)
                        {
                            Line l = (Line)tran.GetObject(
                                id, OpenMode.ForWrite);

                            //get its TO circle
                            //LinkInfoXDataUtil.GetLineLink(_db, id, out fCircleId, out tCircleId);

                            Circle tCircle = (Circle)tran.GetObject(
                                tCircleId, OpenMode.ForRead);

                            l.StartPoint = c.Center;
                            l.EndPoint = tCircle.Center;

                            //Shrink line's length
                            pt = l.GetPointAtDist(c.Radius);
                            l.StartPoint = pt;

                            dist = l.Length - tCircle.Radius;
                            pt = l.GetPointAtDist(dist);
                            l.EndPoint = pt;
                        }

                        //Update TO lines
                        foreach (var id in _toLineIds)
                        {
                            Line l = (Line)tran.GetObject(
                                id, OpenMode.ForWrite);

                            //get its TO circle
                            //LinkInfoXDataUtil.GetLineLink(_db, id, out fCircleId, out tCircleId);

                            Circle fCircle = (Circle)tran.GetObject(fCircleId, OpenMode.ForRead);

                            l.StartPoint = fCircle.Center;
                            l.EndPoint = c.Center;

                            //Shrink line's length
                            pt = l.GetPointAtDist(fCircle.Radius);
                            l.StartPoint = pt;

                            dist = l.Length - c.Radius;
                            pt = l.GetPointAtDist(dist);
                            l.EndPoint = pt;
                        }

                        tran.Commit();
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                HighlightEntities(false);
                DisposeVisualEntities();
            }
        }

        #region Jig method overrides

        protected override bool WorldDraw(WorldDraw draw)
        {
            draw.Geometry.Draw(_visualCircle);

            foreach (var e in _fromVisualLines)
                draw.Geometry.Draw(e);

            foreach (var e in _toVisualLines)
                draw.Geometry.Draw(e);

            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jigOpt =
                new JigPromptPointOptions();

            jigOpt.UserInputControls =
                UserInputControls.Accept3dCoordinates |
                UserInputControls.NoZeroResponseAccepted |
                UserInputControls.NoDwgLimitsChecking;
            jigOpt.BasePoint = _basePoint;
            jigOpt.UseBasePoint = true;
            jigOpt.Cursor = CursorType.RubberBand;
            jigOpt.Message = "\nMove the linked circle to picked point: ";

            PromptPointResult res = prompts.AcquirePoint(jigOpt);

            if (res.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel;
            }
            else
            {
                _currentPoint = res.Value;

                if (_currentPoint == _prevPoint)
                    return SamplerStatus.NoChange;
                else
                {
                    //Update location of visual circle
                    Vector3d displacement = _prevPoint.GetVectorTo(_currentPoint);

                    _visualCircle.TransformBy(
                        Matrix3d.Displacement(displacement));

                    //Update each FROM visual line
                    foreach (var l in _fromVisualLines)
                    {
                        l.StartPoint = _currentPoint;
                    }

                    //Update each TO visual line
                    foreach (var l in _toVisualLines)
                    {
                        l.EndPoint = _currentPoint;
                    }

                    _prevPoint = _currentPoint;
                    return SamplerStatus.OK;
                }
            }
        }

        #endregion

        #region private methods

        private void CreateVisualEntities()
        {
            using (Transaction tran = _db.TransactionManager.StartTransaction())
            {
                //Create visual Circle
                Circle c = (Circle)tran.GetObject(_circleId, OpenMode.ForRead);
                _visualCircle = c.Clone() as Circle;
                _visualCircle.SetDatabaseDefaults(_db);
                _visualCircle.ColorIndex = _visualColorIndex;

                Point3d fromPt = new Point3d();
                Point3d toPt = new Point3d();

                //Create FROM lines
                foreach (var id in _fromLineIds)
                {
                    Line l = (Line)tran.GetObject(id, OpenMode.ForRead);

                    Line vLine = l.Clone() as Line;
                    vLine.SetDatabaseDefaults(_db);
                    vLine.ColorIndex = _visualColorIndex;

                    //Stretch the line to FROM/TO circles' centre points
                    //LinkInfoXDataUtil.GetLinkedCircleCentres(_db, id, out fromPt, out toPt);

                    vLine.StartPoint = fromPt;
                    vLine.EndPoint = toPt;

                    _fromVisualLines.Add(vLine);
                }

                //Create TO lines
                foreach (var id in _toLineIds)
                {
                    Line l = (Line)tran.GetObject(id, OpenMode.ForRead);

                    Line vLine = l.Clone() as Line;
                    vLine.SetDatabaseDefaults(_db);
                    vLine.ColorIndex = _visualColorIndex;

                    //Stretch the line to FROM/TO circles' centre points
                    //LinkInfoXDataUtil.GetLinkedCircleCentres(_db, id, out fromPt, out toPt);

                    vLine.StartPoint = fromPt;
                    vLine.EndPoint = toPt;

                    _toVisualLines.Add(vLine);
                }
            }
        }

        private void HighlightEntities(bool highlight)
        {
            List<ObjectId> ents = new List<ObjectId>();
            ents.Add(_circleId);
            ents.AddRange(_fromLineIds);
            ents.AddRange(_toLineIds);

            //CommonUtil.HighlightEntities(_db, ents, highlight);
        }

        private void DisposeVisualEntities()
        {
            if (_visualCircle != null)
            {
                _visualCircle.Dispose();
                _visualCircle = null;
            }

            foreach (var ent in _fromVisualLines)
                ent.Dispose();

            _fromVisualLines.Clear();

            foreach (var ent in _toVisualLines)
                ent.Dispose();

            _toVisualLines.Clear();
        }

        #endregion
    }
}