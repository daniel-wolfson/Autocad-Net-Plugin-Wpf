namespace Intellidesk.Data.Models.Dto
{
    public class PointArgs
    {
        public double X { set; get; }
        public double Y { set; get; }

        public PointArgs()
        {
        }

        public PointArgs(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}