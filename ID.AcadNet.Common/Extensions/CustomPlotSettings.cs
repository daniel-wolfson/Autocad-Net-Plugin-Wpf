using Autodesk.AutoCAD.DatabaseServices;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public class CustomPlotSettings
    {
        public string MediaName { get; set; } = "none";
        public string MediaNamePair { get; set; } = "";
        public string MediaValue { get; set; } = "none";
        public int MediaWidth { get; set; }
        public int MediaHeight { get; set; }
        public PlotRotation PlotRotation { get; set; }
        //public bool? IsLandscape { get; set; } = null;
        public Extents2d Window { get; set; }
        public string Error { get; set; }
        public bool HasPortrait { get; set; } = false;
        public bool HasLandscape { get; set; } = false;
        public bool HasOversize { get; set; } = false;

        public CustomPlotSettings(string error) {
            Error = error;
        }

        public CustomPlotSettings(string mediaKey, string mediaValue, Extents2d window, PlotRotation plotRotation = PlotRotation.Degrees000)
        {
            MediaName = mediaKey;
            MediaNamePair = mediaKey;
            MediaValue = mediaValue;
            Window = window;
            PlotRotation = plotRotation;
        }

        public double Width()
        {
            return Window.MaxPoint.X - Window.MinPoint.X;
        }
        public double Height()
        {
            return Window.MaxPoint.Y - Window.MinPoint.Y;
        }
    }
}