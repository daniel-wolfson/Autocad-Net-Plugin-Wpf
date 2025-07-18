using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Enums;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Jig
{
    // Create a Class named MyCircleJig that Inherits from EntityJig. 
    // The EntityJig class allows you to "jig" one entity at a time. 
    class CircleJig : EntityJig
    {
        // We need two inputs for a circle, the center and the Radius. 
        public Point3d CenterPoint;
        public double Radius;

        // Because we are going to have 2 inputs, a center point and a Radius we need 
        // to keep track of the input number. 
        private eDragType _currentDragType;

        // Create the default constructor. Pass in an Entity variable named ent. 
        // Derive from the base class and also pass in the ent passed into the constructor. 
        public CircleJig(Entity ent) : base(ent)
        {
        }

        public PromptResult Drag(eDragType dragType) //, out Point3d _currCentre
        {
            //_basePoint = _circle.Center;
            //_currCentre = _circle.Center;
            //_currRadius = _circle.Radius;
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
            Point3d oldPnt = CenterPoint;
            PromptPointResult jigPromptResult = prompts.AcquirePoint("Closure => pick center point : ");

            // Check the status of the PromptPointResult 
            if (jigPromptResult.Status == PromptStatus.OK)
            {
                // Make the CenterPoint member variable equal to the Value 
                // property of the PromptPointResult 
                CenterPoint = jigPromptResult.Value;

                // Check to see if the cursor has moved. 
                if (oldPnt.DistanceTo(CenterPoint) < 0.001)
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
                BasePoint = CenterPoint
            };

            // Now we ready to get input. 
            PromptDoubleResult jigPromptDblResult = prompts.AcquireDistance(jigPromptDistanceOpts);

            //  Check the status of the PromptDoubleResult 
            if (jigPromptDblResult.Status == PromptStatus.OK)
            {
                Radius = jigPromptDblResult.Value;

                // Check to see if the Radius is too small  
                if (System.Math.Abs(Radius) < 0.1)
                {
                    // Make the Member variable Radius = to 1. This is 
                    // just an arbitrary value to keep the circle from being too small 
                    Radius = 1;
                }

                // Check to see if the cursor has moved. 
                if ((System.Math.Abs(oldRadius - Radius) < 0.001))
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
                // Use 0 (zero) for the case. (Updating center for the circle) 
                case eDragType.Location:

                    ((Circle)this.Entity).Center = CenterPoint; // The jig stores the circle as an Entity type. 
                    break; // break out of the switch statement

                // Use 1 for the case. (Updating Radius for the circle) 
                case eDragType.Radius :
                    
                    ((Circle)this.Entity).Radius = Radius; // The jig stores the circle as an Entity type. 
                    break; // break out of the switch statement

            }
            // Return true. 
            return true;
        }
    }
}
