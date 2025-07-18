using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class EllipseJig : EntityJig
    {
        Point3d mCenterPt, mAxisPt, acquiredPoint;
        Vector3d mNormal, mMajorAxis;
        double mRadiusRatio;

        int mPromptCounter;

        DynamicDimensionDataCollection m_dims;

        public EllipseJig(Point3d center, Vector3d vec) : base(new Ellipse())
        {
            mCenterPt = center;
            mNormal = vec;
            mRadiusRatio = 0.00001;
            mPromptCounter = 0;

            m_dims = new DynamicDimensionDataCollection();
            Dimension dim1 = new AlignedDimension();
            dim1.SetDatabaseDefaults();
            m_dims.Add(new DynamicDimensionData(dim1, true, true));
            dim1.DynamicDimension = true;
            Dimension dim2 = new AlignedDimension();
            dim2.SetDatabaseDefaults();
            m_dims.Add(new DynamicDimensionData(dim2, true, true));
            dim2.DynamicDimension = true;

        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jigOpts = new JigPromptPointOptions();
            jigOpts.UserInputControls = (UserInputControls.Accept3dCoordinates | UserInputControls.NoZeroResponseAccepted | UserInputControls.NoNegativeResponseAccepted);

            if (mPromptCounter == 0)
            {
                jigOpts.Message = "\nEllipse Major axis:";
                PromptPointResult dres = prompts.AcquirePoint(jigOpts);

                Point3d axisPointTemp = dres.Value;
                if (axisPointTemp != mAxisPt)
                {
                    mAxisPt = axisPointTemp;
                }
                else
                    return SamplerStatus.NoChange;

                if (dres.Status == PromptStatus.Cancel)
                    return SamplerStatus.Cancel;
                else
                    return SamplerStatus.OK;


            }
            else if (mPromptCounter == 1)
            {
                jigOpts.BasePoint = mCenterPt;
                jigOpts.UseBasePoint = true;
                jigOpts.Message = "\nEllipse Minor axis:";
                double radiusRatioTemp = -1;
                PromptPointResult res = prompts.AcquirePoint(jigOpts);
                acquiredPoint = res.Value;
                radiusRatioTemp = mCenterPt.DistanceTo(acquiredPoint);

                // Ensure the radiusRatio is kept within the expected range.
                if (radiusRatioTemp > 1.0)
                    radiusRatioTemp = 1.0;

                if (radiusRatioTemp != mRadiusRatio)
                    mRadiusRatio = radiusRatioTemp;
                else
                    return SamplerStatus.NoChange;

                if (res.Status == PromptStatus.Cancel)
                    return SamplerStatus.Cancel;
                else
                    return SamplerStatus.OK;

            }
            else
            {
                return SamplerStatus.NoChange;
            }


        }
        protected override bool Update()
        {
            double radiusRatio = 1.0;
            switch (mPromptCounter)
            {
                case 0:
                    // At this time, mAxis contains the value of one
                    // endpoint of the desired major axis.  The
                    // AcDbEllipse class stores the major axis as the
                    // vector from the center point to where the axis
                    // intersects the ellipse path (such as half of the true
                    // major axis), so we already have what we need.
                    //
                    mMajorAxis = mAxisPt - mCenterPt;
                    break;
                case 1:
                    // Calculate the radius ratio.  mRadiusRatio
                    // currently contains the distance from the ellipse
                    // center to the current pointer position.  This is
                    // half of the actual minor axis length.  Since
                    // AcDbEllipse stores the major axis vector as the
                    // vector from the center point to the ellipse curve
                    // (half the major axis), to get the radius ratio we
                    // simply divide the value currently in mRadiusRatio
                    // by the length of the stored major axis vector.
                    //

                    radiusRatio = mRadiusRatio / mMajorAxis.Length;
                    break;
            }

            try
            {
                ((Ellipse)Entity).Set(mCenterPt, new Vector3d(0, 0, 1), mMajorAxis, radiusRatio, 0.0, 6.28318530717958647692);
                UpdateDimensions();

            }
            catch (System.Exception)
            {
                return false;
            }

            return true;

        }
        protected override DynamicDimensionDataCollection GetDynamicDimensionData(double dimScale)
        {
            return m_dims;
        }
        protected override void OnDimensionValueChanged(Autodesk.AutoCAD.DatabaseServices.DynamicDimensionChangedEventArgs e)
        {

        }
        void UpdateDimensions()
        {
            if (mPromptCounter == 0)
            {
                Ellipse myellipse = (Ellipse)Entity;
                AlignedDimension dim = (AlignedDimension)m_dims[0].Dimension;
                dim.XLine1Point = myellipse.Center;
                dim.XLine2Point = mAxisPt;
                dim.DimLinePoint = myellipse.Center;
            }
            else
            {
                Ellipse myellipse = (Ellipse)Entity;
                AlignedDimension dim2 = (AlignedDimension)m_dims[1].Dimension;
                dim2.XLine1Point = myellipse.Center;
                dim2.XLine2Point = acquiredPoint;
                dim2.DimLinePoint = myellipse.Center;
            }

        }
        public void setPromptCounter(int i)
        {
            mPromptCounter = i;
        }
        public Entity GetEntity()
        {
            return Entity;
        }

    }
}