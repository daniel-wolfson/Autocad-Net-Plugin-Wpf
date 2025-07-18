using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common.Internal;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Intellidesk.AcadNet.Services
{
    /// <summary> DrawComplex </summary>
    public class DrawComplex
    {
        /// <summary> Db </summary>
        public static Database Db => HostApplicationServices.WorkingDatabase;

        /// <summary> Circle </summary>
        public static Entity Circle(double tPntX, double tPntY, double tRadius, short tColor = Colors.ByLayer)
        {
            //using (var context = new LayoutContext())
            //{

            //    context.E.Add(new Element()
            //    {
            //        Name = "Shalimar Cricket Ground",
            //        Location = DbGeography.FromText("POINT(-122.336106 47.605049)"),
            //    });

            //    context.CricketGrounds.Add(new CricketGround()
            //    {
            //        Name = "Marghazar Stadium",
            //        Location = DbGeography
            //            .FromText("POINT(-122.335197 47.646711)"),
            //    });

            //    context.SaveChanges();

            //    var myLocation = DbGeography.FromText("POINT(-122.296623 47.640405)");

            //    var cricketGround = (from cg in context.CricketGrounds
            //        orderby cg.Location.Distance(myLocation)
            //        select cg).FirstOrDefault();

            //    Console.WriteLine("The closest Cricket Ground to you is: {0}.", cricketGround.Name);
            return null;
        }

        public static void ConvertTextToImage(string txtText)
        {
            string text = txtText.Trim();
            Bitmap bitmap = new Bitmap(1, 1);
            System.Drawing.Font font = new System.Drawing.Font("Arial", 25, FontStyle.Regular, GraphicsUnit.Pixel);
            Graphics graphics = Graphics.FromImage(bitmap);
            int width = (int)graphics.MeasureString(text, font).Width;
            int height = (int)graphics.MeasureString(text, font).Height;
            bitmap = new Bitmap(bitmap, new Size(width, height));
            graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            int argb = Color.Black.ToArgb();
            graphics.DrawString(text, font, new SolidBrush(Color.FromArgb(argb)), 0, 0);
            graphics.Flush();
            graphics.Dispose();
            bitmap.Save(String.Format("C:\\Temp\\Image_1 {0}.png", "ZIGZAG"), ImageFormat.Png);


        }
    }
}
