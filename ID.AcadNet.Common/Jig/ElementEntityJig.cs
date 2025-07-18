using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Jig
{
    /// <summary>
    /// Create a Class named MyCircleJig that Inherits from EntityJig. 
    /// The EntityJig class allows you to "jig" one entity at a time. 
    /// </summary>
    public class ElementEntityJig : EntityJig
    {
        public Point3d BasePoint { get; private set; } // center for circle; start point for curve, vertex[o] for polyline 
        public double Radius { get; private set; }
        public IPaletteElement Element { get; private set; }

        // Because we are going to have 2 inputs, a center point and a Radius we need 
        // to keep track of the input number. 
        private eDragType _currentDragType;

        // Create the default constructor. Pass in an Entity variable named ent. 
        // Derive from the base class and also pass in the ent passed into the constructor. 
        public ElementEntityJig(Entity ent, IPaletteElement element) : base(ent)
        {
            Element = element;
        }

        public PromptResult Drag(eDragType dragType) //, out Point3d _currCentre
        {
            var ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            _currentDragType = dragType;

            return ed.Drag(this);
        }

        // Override the Sampler function.
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            // Create a switch statement. 
            switch (_currentDragType)
            {
                case eDragType.Location:
                    return LocationSampler(prompts);
                case eDragType.Radius:
                    return LocationSampler(prompts);
            }

            // Return SamplerSataus.NoChange. This will not ever be hit as we are returning
            // in the switch statement. (just avoiding the compile error)
            return SamplerStatus.NoChange;
        }

        protected SamplerStatus LocationSampler(JigPrompts prompts)
        {
            Point3d oldPnt = BasePoint;
            PromptPointResult jigPromptResult = prompts.AcquirePoint("Pick center point : ");

            // Check the status of the PromptPointResult 
            if (jigPromptResult.Status == PromptStatus.OK)
            {
                // Make the CenterPoint member variable equal to the Value 
                // property of the PromptPointResult 
                BasePoint = jigPromptResult.Value;

                // Check to see if the cursor has moved. 
                if (oldPnt.DistanceTo(BasePoint) < 0.001)
                {
                    // If we get here then there has not been any change to the location 
                    // return SamplerStatus.NoChange 
                    return SamplerStatus.NoChange;
                }
            }

            // If the code gets here than there has been a change in the location so 
            // return SamplerStatus.OK 
            return SamplerStatus.OK;
        }

        protected SamplerStatus RadiusSampler(JigPrompts prompts)
        {
            double oldRadius = Radius;
            JigPromptDistanceOptions jigPromptDistanceOpts = new JigPromptDistanceOptions("Pick Radius : ")
            {
                UseBasePoint = true,
                BasePoint = BasePoint
            };

            // Now we ready to get input. 
            PromptDoubleResult jigPromptDblResult = prompts.AcquireDistance(jigPromptDistanceOpts);

            //  Check the status of the PromptDoubleResult 
            if (jigPromptDblResult.Status == PromptStatus.OK)
            {
                Radius = jigPromptDblResult.Value;

                // Check to see if the Radius is too small  
                if (Math.Abs(Radius) < 0.1)
                {
                    // Make the Member variable Radius = to 1. This is 
                    // just an arbitrary value to keep the circle from being too small 
                    Radius = 1;
                }

                // Check to see if the cursor has moved. 
                if (Math.Abs(oldRadius - Radius) < 0.001)
                {
                    // If we get here then there has not been any change to the location 
                    // Return SamplerStatus.NoChange 
                    return SamplerStatus.NoChange;
                }
            }

            // If we get here the cursor has moved. return SamplerStatus.OK 
            return SamplerStatus.OK;
        }

        // Override the Update function. 
        protected override bool Update()
        {
            // In this function (Update) for every input, we need to update the entity 
            switch (_currentDragType)
            {
                case eDragType.Location:

                    if (Entity.GetType() == typeof(Circle))
                        ((Circle)this.Entity).Center = BasePoint;  // The jig stores the circle as an Entity type. 
                    else if (Entity.GetType() == typeof(Polyline) && Element.GetType() == typeof(AcadCabinet))
                    {
                        Polyline pline = Entity as Polyline;
                        //polyline.Closed = true;
                        if (pline != null)
                        {
                            //Extents3d boundary = pline.GeometricExtents;
                            //Point3d center = new LineSegment3d(boundary.MinPoint, boundary.MaxPoint).MidPoint;
                            //pline.TransformBy(Matrix3d.Scaling(1, center + new Vector3d(Element.Width, Element.Height / 2, 0)));

                            for (int i = 0; i < pline.NumberOfVertices; i++)
                            {
                                Point3d pt = pline.GetPoint2dAt(i).XGetPoint3d();
                                Matrix3d mat = Matrix3d.Displacement(
                                    pt.GetVectorTo(BasePoint.Add(new Vector3d((double)Element.Width / 2, (double)Element.Height / 2, 0))));
                                //pline.SetPointAt(i, ptToRotate.TransformBy(Matrix2d.Scaling(1, center.XGetPoint2d())));
                                pline.TransformBy(mat);
                            }
                        }
                    }

                    break;

                case eDragType.Radius:

                    if (Entity.GetType() == typeof(Circle))
                        ((Circle)this.Entity).Radius = Radius; // The jig stores the circle as an Entity type. 

                    break;
            }

            return true;
        }

        public Entity GetEntity()
        {
            return this.Entity;
        }

    }
}
