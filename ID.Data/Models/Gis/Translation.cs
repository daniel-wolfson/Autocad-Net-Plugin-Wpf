using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Gis
{
    public class Translation
    {
        public double dx { get; set; }
        public double dy { get; set; }
        public double dz { get; set; }

        public Translation(double dx, double dy, double dz)
        {
            this.dx = dx;
            this.dy = dy;
            this.dz = dz;
        }

        public Cad.Point translate(Cad.Point point)
        {
            return new Cad.Point(point.X + this.dx, point.Y + this.dy, point.Z + this.dz);
        }
    }
}