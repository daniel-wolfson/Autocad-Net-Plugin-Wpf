using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Intellidesk.AcadNet.Commands.Jig;
using System;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.Commands.Draw
{
    public class Commands
    {
        [CommandMethod("BOJ")]

        public void BoxJig()
        {
            var doc =
              Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            // Let's get the initial corner of the box

            var ppr = ed.GetPoint("\nSpecify first corner: ");

            if (ppr.Status == PromptStatus.OK)
            {
                // In order for the visual style to be respected,
                // we'll add the to-be-jigged solid to the database

                var tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    var btr =
                      (BlockTableRecord)tr.GetObject(
                        db.CurrentSpaceId, OpenMode.ForWrite
                      );

                    var sol = new Solid3d();
                    btr.AppendEntity(sol);
                    tr.AddNewlyCreatedDBObject(sol, true);

                    // Create our jig object passing in the selected point

                    var jf =
                      new EntityJigFramework(
                        ed.CurrentUserCoordinateSystem, sol, ppr.Value,
                        new List<Phase>()
                        {

                // Two phases, the second of which has a custom
                // offset for the base point

                new PointPhase("\nSpecify opposite corner: "),
                new SolidDistancePhase(
                  "\nSpecify height: ",
                  1e-05,
                  (vals, pt) =>
                  {
                    // Get the diagonal line between the corners
                    var pt2 = (Point3d)vals[0].Value;
                    var diag = pt2 - pt;
                    var dlen = diag.Length;

                    // Use Pythagoras' theorem to get the side
                    // length

                    var side = Math.Sqrt(dlen * dlen / 2);
                    var halfSide = side / 2;

                    // Start by getting the displacement from
                    // the start point, adjusting for the fact
                    // we're jigging from a corner, not the center

                    var mat =
                      Matrix3d.Displacement(
                        pt.GetAsVector() +
                        new Vector3d(
                          halfSide,
                          halfSide,
                          (double)vals[1].Value / 2
                        )
                      );

                    // Calculate the angle between the diagonal
                    // and the X axis

                    var ang =
                      JigUtils.ComputeAngle(
                        pt,
                        pt2,
                        Vector3d.XAxis,
                        Matrix3d.Identity
                      );

                    // Add a rotation component to the
                    // transformation, adjusted by 45 degrees
                    // to jig the box diagonally

                    mat =
                      mat.PostMultiplyBy(
                        Matrix3d.Rotation(
                          ang + (-45 * Math.PI / 180),
                          Vector3d.ZAxis,
                          new Point3d(-halfSide, -halfSide, 0)
                        )
                      );

                    return
                      new Vector3d(halfSide, halfSide, 0).
                        TransformBy(mat);
                  }
                )
                        },
                        (e, vals, pt, ucs) =>
                        {

                            // Our entity update function
                            // Get the diagonal line between the corners

                            var pt2 = (Point3d)vals[0].Value;
                            var diag = pt2 - pt;
                            var dlen = diag.Length;

                            // Use Pythagoras' theorem to get the side
                            // length

                            var side = Math.Sqrt(dlen * dlen / 2);
                            var halfSide = side / 2;

                            // Create our box with square sides and
                            // the chosen height

                            var s = (Solid3d)e;
                            s.CreateBox(side, side, (double)vals[1].Value);

                            // Start by getting the displacement from
                            // the start point, adjusting for the fact
                            // we're jigging from a corner, not the center
                            // (need to adjust for the current UCS)

                            var mat =
                              Matrix3d.Displacement(
                                pt.GetAsVector() +
                                new Vector3d(
                                  halfSide,
                                  halfSide,
                                  (double)vals[1].Value / 2
                                )
                              ).PreMultiplyBy(ucs);

                            // Calculate the angle between the diagonal
                            // and the X axis

                            var ang =
                              JigUtils.ComputeAngle(
                                pt,
                                pt2,
                                Vector3d.XAxis,
                                Matrix3d.Identity
                              );

                            // Add a rotation component to the
                            // transformation, adjusted by 45 degrees
                            // to jig the box diagonally

                            mat =
                              mat.PostMultiplyBy(
                                Matrix3d.Rotation(
                                  ang + (-45 * Math.PI / 180),
                                  Vector3d.ZAxis,
                                  new Point3d(-halfSide, -halfSide, 0)
                                )
                              );

                            // Transform our solid
                            s.TransformBy(mat);
                            return true;
                        }
                      );

                    jf.RunTillComplete(ed, tr);
                }
            }
        }


    }
}
