using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class TextPlacementJig : EntityJig
    {
        // Declare some internal state

        private readonly Transaction _tr;
        private readonly Database _db;
        private Point3d _position;
        private Point3d _basePosition;
        private readonly Editor _ed;

        private eDragType _dragType;
        private readonly bool _transactionAbortEnabled;

        private static DBText _textPrototype;
        private DBText _text;
        private double _angle;
        private double _baseAngle;
        private TextHorizontalMode _horizontalMode;
        private double _txtSize;
        private bool _isUndo;

        // ctor
        public TextPlacementJig(Database db, DBText ent, bool transactionAbortEnabled = true) : base(ent)
        {
            if (_textPrototype == null)
                _textPrototype = (DBText)ent.Clone();
            _db = db;
            _ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            _angle = ent.Rotation;
            _txtSize = ent.Height;
            _horizontalMode = ent.HorizontalMode;
        }

        protected override SamplerStatus Sampler(JigPrompts jp)
        {
            Extents3d extents = ((DBText)Entity).GeometricExtents;
            var width = extents.MaxPoint.X - extents.MinPoint.X;

            //Point3d center = new LineSegment3d(boundary.MinPoint, boundary.MaxPoint).MidPoint;
            //text.TransformBy(Matrix3d.Scaling(1, center));

            JigPromptPointOptions po = new JigPromptPointOptions("\nPosition of text");

            po.UserInputControls =
                (UserInputControls.Accept3dCoordinates |
                 UserInputControls.NullResponseAccepted |
                 UserInputControls.NoNegativeResponseAccepted |
                 UserInputControls.GovernedByOrthoMode);

            po.SetMessageAndKeywords("\nSpecify position of text or " +
                "[Regular/Bold/Italic/LArger/Smaller/?Rotate/90Rotate/LEft/Middle/RIght]: ",
                "Regular Bold Italic LArger Smaller ?Rotate 90Rotate LEft Middle RIght");

            PromptPointResult ppr = jp.AcquirePoint(po);

            if (ppr.Status == PromptStatus.Keyword)
            {
                switch (ppr.StringResult)
                {
                    case "Regular":
                        {
                            _angle = 0;
                            _horizontalMode = TextHorizontalMode.TextLeft;
                            _txtSize = 0.85;
                            _isUndo = true;
                            break;
                        }
                    case "Bold":
                        {
                            // TODO
                            break;
                        }
                    case "Italic":
                        {
                            // TODO
                            break;
                        }
                    case "LArger":
                        {
                            // Multiple the text size by two
                            _txtSize *= 2;
                            break;
                        }
                    case "Smaller":
                        {
                            // Divide the text size by two
                            _txtSize /= 2;
                            break;
                        }
                    case "?Rotate":
                        {
                            RotateJig jig = new RotateJig(Entity, _position, _baseAngle, _db.GetUcsMatrix());
                            PromptResult res = _ed.Drag(jig);
                            _angle = res.Status == PromptStatus.OK
                                ? jig.GetRotation()
                                : 0;
                            break;
                        }
                    case "90Rotate":
                        {
                            // To rotate clockwise we subtract 90 degrees and
                            // then normalise the angle between 0 and 360
                            _angle -= Math.PI / 2;
                            while (_angle < Math.PI * 2)
                            {
                                _angle += Math.PI * 2;
                            }
                            break;
                        }
                    case "LEft":
                        {
                            _horizontalMode = TextHorizontalMode.TextLeft;
                            break;
                        }
                    case "RIght":
                        {
                            _horizontalMode = TextHorizontalMode.TextRight;
                            break;
                        }
                    case "Middle":
                        {
                            _horizontalMode = TextHorizontalMode.TextMid;
                            break;
                        }
                }

                return SamplerStatus.OK;
            }

            if (ppr.Status == PromptStatus.OK)
            {
                // Check if it has changed or not (reduces flicker)
                if (_position.DistanceTo(ppr.Value) < Tolerance.Global.EqualPoint)
                    return SamplerStatus.NoChange;

                _position = ppr.Value;
                return SamplerStatus.OK;
            }

            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            Extents3d extents = ((DBText)Entity).GeometricExtents;
            var width = extents.MaxPoint.X - extents.MinPoint.X;

            _text = (DBText)Entity;

            _text.Position = _position;
            _text.Height = _txtSize;
            _text.Rotation = _angle;
            _text.HorizontalMode = _horizontalMode;

            if (_isUndo)
            {
                _text.Height = _textPrototype.Height;
                _text.Rotation = _textPrototype.Rotation;
                _text.HorizontalMode = _textPrototype.HorizontalMode;
                _isUndo = false;
            }
            else
            if (_horizontalMode == TextHorizontalMode.TextRight)
            {
                _basePosition = _position + new Vector3d(-width, 0, 0);
            }
            else if (_horizontalMode == TextHorizontalMode.TextMid)
            {
                _basePosition = _position + new Vector3d(-width / 2, 0, 0);
            }

            if (_horizontalMode != TextHorizontalMode.TextLeft)
            {
                _text.AlignmentPoint = _basePosition;
                _text.Position = _basePosition;
            }

            return true;
        }

        public PromptStatus Drag(eDragType dragType = eDragType.Location)
        {
            _dragType = dragType;

            PromptStatus promptStatus = PromptStatus.Keyword;
            while (promptStatus == PromptStatus.Keyword)
            {
                PromptResult result = _ed.Drag(this);
                promptStatus = result.Status;
            }

            if (promptStatus != PromptStatus.OK) // && stat != PromptStatus.Keyword
            {
                //if (_transactionAbortEnabled) _tr?.Abort();
                //Entity.UpgradeOpen();
                //Entity.Dispose();
                //pr = PromptStatus.Cancel;
            }
            return promptStatus;
        }

        public DBText GetEntity()
        {
            DBText dbText = (DBText)_text.Clone();
            if (_horizontalMode != TextHorizontalMode.TextLeft)
                dbText.AlignmentPoint = _position;
            dbText.Position = _position;
            dbText.Height = _txtSize;
            dbText.Rotation = _angle;
            return dbText;
        }

        public Point3d GetPosition()
        {
            return _position;
        }

        public static void Clear()
        {
            _textPrototype = null;
        }
    }
}