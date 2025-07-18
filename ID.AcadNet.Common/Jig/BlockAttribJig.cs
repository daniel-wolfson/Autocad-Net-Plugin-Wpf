using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class BlockAttribJig : BlockJig
    {
        struct TextInfo
        {
            public Point3d Position;
            public Point3d Alignment;
            public double Rotation;
            public bool IsAligned;
        }

        private Dictionary<string, TextInfo> _attInfos;

        public BlockAttribJig(BlockReference br)
            : base(br)
        {
            _attInfos = new Dictionary<string, TextInfo>();
            BlockTableRecord btr = (BlockTableRecord)br.BlockTableRecord.GetObject(OpenMode.ForRead);
            foreach (ObjectId id in btr)
            {
                if (id.ObjectClass.Name == "AcDbAttributeDefinition")
                {
                    AttributeDefinition attDef = (AttributeDefinition)id.GetObject(OpenMode.ForRead);
                    TextInfo ti = new TextInfo()
                    {
                        Position = attDef.Position,
                        Alignment = attDef.AlignmentPoint,
                        IsAligned = attDef.Justify != AttachmentPoint.BaseLeft,
                        Rotation = attDef.Rotation
                    };
                    _attInfos.Add(attDef.Tag.ToUpper(), ti);
                }
            }
        }

        protected override bool Update()
        {
            base.Update();
            foreach (ObjectId id in _br.AttributeCollection)
            {
                AttributeReference att = (AttributeReference)id.GetObject(OpenMode.ForWrite);
                att.Rotation = _br.Rotation;
                string tag = att.Tag.ToUpper();
                if (_attInfos.ContainsKey(tag))
                {
                    TextInfo ti = _attInfos[tag];
                    att.Position = ti.Position.TransformBy(_br.BlockTransform);
                    if (ti.IsAligned)
                    {
                        att.AlignmentPoint =
                            ti.Alignment.TransformBy(_br.BlockTransform);
                        att.AdjustAlignment(_br.Database);
                    }
                    if (att.IsMTextAttribute)
                    {
                        att.UpdateMTextAttribute();
                    }
                    att.Rotation = ti.Rotation + _br.Rotation;
                }
            }
            return true;
        }
    }
}