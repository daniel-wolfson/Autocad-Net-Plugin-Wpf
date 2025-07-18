using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Extensions;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Intellidesk.AcadNet.Common.Jig
{//PolylineJig
    public sealed class PolylineJig : EntityJig, IDisposable
    {
        #region Variables & Properties

        private readonly Point3dCollection _point3DCollection;
        private readonly Plane _plane;
        private readonly Matrix3d _ucs;
        private Point3d _mTempPoint;
        private readonly Document _document;
        private readonly Editor _editor;
        private readonly Database _database;
        private ObjectId _snapPlineOid = ObjectId.Null;
        private readonly KeywordCollection _keywordCollection;

        public Polyline Polyline => Entity as Polyline;

        public string Prompt { get; set; }

        #endregion

        public PolylineJig(int colorIndex = 0) : base(new Polyline() { ColorIndex = colorIndex })
        {
            _document = acadApp.DocumentManager.MdiActiveDocument;
            _editor = _document.Editor;
            _database = _document.Database;
            _point3DCollection = new Point3dCollection(); // Create a point collection to store our vertices
            _ucs = _editor.CurrentUserCoordinateSystem; // Get the current UCS

            // Create a temporary plane, to help with calcs
            var normal = new Vector3d(0, 0, 1);
            normal = normal.TransformBy(_ucs);
            _plane = new Plane(new Point3d(0, 0, 0), normal);

            // Create polyline, set defaults, add dummy vertex
            var pline = Entity as Polyline;
            if (pline != null)
            {
                pline.SetDatabaseDefaults();
                pline.Normal = normal;
                pline.AddVertexAt(0, new Point2d(0, 0), 0, 0.2, 0.2);
            }
        }

        public PolylineJig(string prompt, int colorIndex = 0) : this(colorIndex)
        {
            Prompt = prompt;
        }

        public PolylineJig(string prompt, KeywordCollection keywordCollection, int colorIndex = 0) : this(prompt, colorIndex)
        {
            if (keywordCollection == null)
                throw new ArgumentNullException("keywordCollection");

            _keywordCollection = keywordCollection;
        }

        protected override bool Update()
        {
            // Update the dummy vertex to be our 3D point projected onto our plane
            Polyline?.SetPointAt(Polyline.NumberOfVertices - 1, _mTempPoint.Convert2d(_plane));
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jigOpts = new JigPromptPointOptions
            {
                UserInputControls = UserInputControls.Accept3dCoordinates |
                                    UserInputControls.NullResponseAccepted |
                                    UserInputControls.NoNegativeResponseAccepted
            };

            if (_point3DCollection.Count == 0)
            {
                jigOpts.Message = "\n" + Prompt; // For the first vertex, just ask for the point
            }
            else if (_point3DCollection.Count > 0)
            {
                // For subsequent vertices, use a base point
                jigOpts.BasePoint = _point3DCollection[_point3DCollection.Count - 1];
                jigOpts.UseBasePoint = true;
                jigOpts.Message = "\n" + Prompt;

                if (_keywordCollection != null)
                {
                    foreach (Keyword keyword in _keywordCollection)
                    {
                        jigOpts.Keywords.Add(keyword.GlobalName, keyword.LocalName, keyword.DisplayName, keyword.Visible,
                        keyword.Enabled);
                    }
                }
            }
            else // should never happen
                return SamplerStatus.Cancel;

            // Get the point itself
            PromptPointResult res = prompts.AcquirePoint(jigOpts);

            // Check if it has changed or not
            // (reduces flicker)
            if (_mTempPoint == res.Value)
            {
                return SamplerStatus.NoChange;
            }

            if (res.Status == PromptStatus.OK)
            {
                _mTempPoint = res.Value;
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }

        private void RemoveLastVertex()
        {
            if (Polyline != null)
            {
                if (_point3DCollection.Count > 0)
                {
                    Polyline.RemoveVertexAt(_point3DCollection.Count);
                    _point3DCollection.RemoveAt(_point3DCollection.Count);
                }
            }
        }

        private void AddLatestVertex()
        {
            // Add the latest selected point to
            // our internal list...
            // This point will already be in the
            // most recently added pline vertex
            _point3DCollection.Add(_mTempPoint);
            // Create a new dummy vertex...
            // can have any initial value
            Polyline?.AddVertexAt(Polyline.NumberOfVertices, new Point2d(0, 0), 0, 0.2, 0.2);
        }

        public void Dispose()
        {
            RemoveLastVertex();
            Remove();
            _point3DCollection.Dispose();
            _plane.Dispose();
        }

        private void Remove()
        {
            //RemoveLastVertex();

            if (_snapPlineOid != ObjectId.Null)
            {
                using (var tr = _database.TransactionManager.StartTransaction())
                {
                    var pl = tr.GetObject(_snapPlineOid, OpenMode.ForWrite) as Polyline;
                    pl?.Erase(true);
                    _snapPlineOid = ObjectId.Null;
                }
            }
        }

        public void Draw(short colorIndex = 0)
        {
            AddLatestVertex();

            if (_snapPlineOid != ObjectId.Null)
                Remove();

            if (Polyline.NumberOfVertices > 2)
            {
                var snapPline = new Polyline() { ColorIndex = colorIndex };
                int i = 0;
                foreach (Point3d pt in _point3DCollection)
                {
                    snapPline.AddVertexAt(i, new Point2d(pt.X, pt.Y), 0, 0.2, 0.2);
                    snapPline.SetBulgeAt(i, -0.5);
                    i++;
                }

                snapPline.XSaveChanges();
                _snapPlineOid = snapPline.ObjectId;
            }
        }

        public void Undo()
        {
            RemoveLastVertex();
            Remove();
        }


        public void Close()
        {
            if (_point3DCollection.Count > 2)
            {
                Polyline.AddVertexAt(Polyline.NumberOfVertices,
                    new Point2d(_point3DCollection[0].X, _point3DCollection[0].Y), 0, 0.2, 0.2);
            }

        }

    }
}
