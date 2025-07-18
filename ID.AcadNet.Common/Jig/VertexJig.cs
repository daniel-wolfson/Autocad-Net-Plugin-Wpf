using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class VertexJig : EntityJig
    {
        Polyline m_pline;
        Point3d m_point;
        int m_index;
        Vector3d m_vector;
        double m_bulge;
        double m_sWidth;
        double m_eWidth;

        public VertexJig(Polyline pline, Point3d point, int index, Vector3d vector, double bulge, double sWidth, double eWidth) : base(pline)
        {
            m_pline = pline;
            m_point = point;
            m_index = index;
            m_vector = vector;
            m_bulge = bulge;
            m_sWidth = sWidth;
            m_eWidth = eWidth;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jppo = new JigPromptPointOptions("\nSpecify the new vertex: ");
            jppo.UserInputControls = (UserInputControls.Accept3dCoordinates);
            PromptPointResult ppr = prompts.AcquirePoint(jppo);
            if (ppr.Status == PromptStatus.OK)
            {
                if (ppr.Value.IsEqualTo(m_point))
                    return SamplerStatus.NoChange;
                else
                {
                    m_point = ppr.Value;
                    return SamplerStatus.OK;
                }
            }
            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            if (m_pline.NumberOfVertices == 3)
            {
                Point3d transPt = m_point.TransformBy(Matrix3d.WorldToPlane(m_pline.Normal));
                Point2d pt = new Point2d(transPt.X, transPt.Y);
                double length = m_pline.GetDistanceAtParameter(2);
                double dist1 = m_pline.GetDistanceAtParameter(1);
                double dist2 = length - dist1;
                double width = m_sWidth < m_eWidth ?
                    ((dist1 * (m_eWidth - m_sWidth)) / length) + m_sWidth :
                    ((dist2 * (m_sWidth - m_eWidth)) / length) + m_eWidth;
                double angle = Math.Atan(m_bulge);
                m_pline.SetPointAt(m_index, pt);
                m_pline.SetEndWidthAt(0, width);
                m_pline.SetStartWidthAt(1, width);
                m_pline.SetBulgeAt(0, Math.Tan(angle * (dist1 / length)));
                m_pline.SetBulgeAt(1, Math.Tan(angle * (dist2 / length)));
            }
            else if (m_index == 0)
            {
                Point3d transPt = m_point.TransformBy(Matrix3d.WorldToPlane(m_pline.Normal));
                Point2d pt = new Point2d(transPt.X, transPt.Y);
                m_pline.SetPointAt(m_index, pt);
                if (m_bulge != 0.0)
                {
                    Vector3d vec = m_point.GetVectorTo(m_pline.GetPoint3dAt(1));
                    double ang = vec.GetAngleTo(m_vector, m_pline.Normal);
                    double bulge = Math.Tan(ang / 2.0);
                    m_pline.SetBulgeAt(0, bulge);
                }
            }
            else
            {
                Point3d transPt = m_point.TransformBy(Matrix3d.WorldToPlane(m_pline.Normal));
                Point2d pt = new Point2d(transPt.X, transPt.Y);
                m_pline.SetPointAt(m_index, pt);
                if (m_bulge != 0.0)
                {
                    Vector3d vec = m_pline.GetPoint3dAt(0).GetVectorTo(m_point);
                    double ang = m_vector.GetAngleTo(vec, m_pline.Normal);
                    double bulge = Math.Tan(ang / 2.0);
                    m_pline.SetBulgeAt(0, bulge);
                }
            }
            return true;
        }

        public Point3d GetPoint()
        {
            return m_point;
        }
    }
}