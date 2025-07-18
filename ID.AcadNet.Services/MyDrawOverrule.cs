using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Intellidesk.AcadNet.Services
{
    public class MyDrawOverrule : DrawableOverrule
    {
        public override bool WorldDraw(Drawable drawable, WorldDraw wd)
        {
            // Cast Drawable to Line so we can access its methods and
            // properties
            Line ln = (Line)drawable;

            // Draw some graphics primitives
            wd.Geometry.Circle(ln.StartPoint + 0.5 * ln.Delta, ln.Length / 5, ln.Normal);

            // In this case we don't want the line to draw itself, nor do
            // we want ViewportDraw called
            return true;
        }
    }
}