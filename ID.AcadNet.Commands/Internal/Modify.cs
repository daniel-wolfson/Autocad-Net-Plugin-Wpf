using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Intellidesk.AcadNet.Commands.Common;
using Intellidesk.AcadNet.Commands.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Intellidesk.AcadNet.Commands.Drawing
{
    /// <summary>
    /// The "Modify" module: edit entities (with AutoCAD-command-like functions)
    /// </summary>
    public static class Modify // todo: need to add overloads for multiple ids
    {
        #region geometric

        /// <summary>
        /// Moves an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="displacement">The displacement.</param>
        public static void Move(this ObjectId entityId, Vector3d displacement)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                entity.TransformBy(Matrix3d.Displacement(displacement));
                trans.Commit();
            }
        }

        /// <summary>
        /// Moves an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="displacement">The displacement.</param>
        public static void Move(this Entity entity, Vector3d displacement)
        {
            entity.TransformBy(Matrix3d.Displacement(displacement));
        }

        /// <summary>
        /// Copies an entity given a list of displacement.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="displacements">The displacements.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Copy(this ObjectId entityId, IEnumerable<Vector3d> displacements)
        {
            var db = entityId.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                var currentSpace = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                var copyIds = displacements
                    .Select(vector =>
                    {
                        var copy = entity.Clone() as Entity;
                        copy.TransformBy(Matrix3d.Displacement(vector));
                        var id = currentSpace.AppendEntity(copy);
                        trans.AddNewlyCreatedDBObject(copy, true);
                        return id;
                    })
                    .ToArray();

                trans.Commit();
                return copyIds;
            }
        }

        /// <summary>
        /// Copies an entity given a base point and a list of new points.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="newPoints">The new points.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Copy(this ObjectId entityId, Point3d basePoint, IEnumerable<Point3d> newPoints)
        {
            return entityId.Copy(newPoints.Select(newPoint => newPoint - basePoint));
        }

        /// <summary>
        /// Rotates an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="center">The center.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="axis">The axis. Default is the Z axis.</param>
        public static void Rotate(this ObjectId entityId, Point3d center, double angle, Vector3d? axis = null)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                entity.TransformBy(Matrix3d.Rotation(angle, axis ?? Vector3d.ZAxis, center));
                trans.Commit();
            }
        }

        /// <summary>
        /// Scales an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="scale">The scale.</param>
        public static void Scale(this ObjectId entityId, Point3d basePoint, double scale)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                entity.TransformBy(Matrix3d.Scaling(scale, basePoint));
                trans.Commit();
            }
        }

        /// <summary>
        /// Offsets a curve.
        /// </summary>
        /// <param name="curveId">The curve ID.</param>
        /// <param name="distance">The offset distance.</param>
        /// <param name="side">A point to indicate which side.</param>
        /// <returns>The objecct ID.</returns>
        public static ObjectId Offset(this ObjectId curveId, double distance, Point3d side)
        {
            var curve = curveId.QOpenForRead<Curve>();
            var curve1 = curve.GetOffsetCurves(-distance)[0] as Curve;
            var curve2 = curve.GetOffsetCurves(distance)[0] as Curve;
            return Drawing.AddToCurrentSpace(curve1.GetDistToPoint(side) < curve2.GetDistToPoint(side)
                ? curve1
                : curve2);
        }

        /// <summary>
        /// Mirrors an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="copy">Whether to copy.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Mirror(this ObjectId entityId, Line axis, bool copy = true)
        {
            var entity = entityId.QOpenForRead<Entity>();
            var axisLine = new Line3d(axis.StartPoint, axis.EndPoint);
            var mirror = entity.Clone() as Entity;
            mirror.TransformBy(Matrix3d.Mirroring(axisLine));
            if (!copy)
            {
                entityId.Erase();
            }
            return Drawing.AddToCurrentSpace(mirror);
        }

        /// <summary>
        /// Breaks a curve.
        /// </summary>
        /// <param name="curveId">The curve ID.</param>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Break(this ObjectId curveId, Point3d point1, Point3d point2)
        {
            using (var trans = curveId.Database.TransactionManager.StartTransaction())
            {
                var curve = trans.GetObject(curveId, OpenMode.ForRead) as Curve;
                var param1 = curve.GetParamAtPointX(point1);
                var param2 = curve.GetParamAtPointX(point2);
                var splits = curve.GetSplitCurves(new DoubleCollection(new[] { param1, param2 }));
                var breaks = splits.Cast<Entity>();
                return new[] { breaks.First().ObjectId, breaks.Last().ObjectId };
            }
        }

        /// <summary>
        /// Breaks a curve at point.
        /// </summary>
        /// <param name="curveId">The curve ID.</param>
        /// <param name="position">The position.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Break(this ObjectId curveId, Point3d position)
        {
            return curveId.Break(position, position);
        }

        #endregion

        #region database

        /// <summary>
        /// Explodes an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Explode(this ObjectId entityId)
        {
            var entity = entityId.QOpenForRead<Entity>();
            var results = new DBObjectCollection();
            entity.Explode(results);
            entityId.Erase();
            return results
                .Cast<Entity>()
                .Select(newEntity => newEntity.AddToCurrentSpace())
                .ToArray(); ;
        }

        /// <summary>
        /// Erases an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        public static void Erase(this ObjectId entityId)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite);
                entity.Erase();
                trans.Commit();
            }
        }

        /// <summary>
        /// Deletes a group and erases the entities.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        public static void EraseGroup(this ObjectId groupId)
        {
            DbHelper
                .GetEntityIdsInGroup(groupId)
                .Cast<ObjectId>()
                .QForEach(entity => entity.Erase());

            groupId.QOpenForWrite(group => group.Erase());
        }

        /// <summary>
        /// Groups entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="name">The group name.</param>
        /// <param name="selectable">Whether to allow select.</param>
        /// <returns>The group ID.</returns>
        public static ObjectId Group(this IEnumerable<ObjectId> entityIds, string name = "*", bool selectable = true)
        {
            var db = DbHelper.GetDatabase(entityIds);
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var groupDict = (DBDictionary)trans.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);
                var group = new Group(name, selectable);
                foreach (var id in entityIds)
                {
                    group.Append(id);
                }
                var result = groupDict.SetAt(name, group);
                trans.AddNewlyCreatedDBObject(group, true); // false with no commit?
                trans.Commit();
                return result;
            }
        }

        /// <summary>
        /// Appends entities to group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <param name="entityIds">The entity IDs.</param>
        public static void AppendToGroup(ObjectId groupId, params ObjectId[] entityIds)
        {
            using (var trans = DbHelper.GetDatabase(entityIds).TransactionManager.StartTransaction())
            {
                var group = trans.GetObject(groupId, OpenMode.ForWrite) as Group;
                if (group != null)
                {
                    group.Append(new ObjectIdCollection(entityIds));
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// Ungroups a group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        public static void Ungroup(ObjectId groupId)
        {
            var groupDictId = groupId.Database.GroupDictionaryId;
            groupDictId.QOpenForWrite<DBDictionary>(groupDict => groupDict.Remove(groupId));
            groupId.Erase();
        }

        /// <summary>
        /// Ungroups a group by entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <returns>The number of groups ungrouped.</returns>
        public static int Ungroup(IEnumerable<ObjectId> entityIds)
        {
            var groupDict = DbHelper.GetDatabase(entityIds).GroupDictionaryId.QOpenForRead<DBDictionary>();
            var count = 0;
            foreach (var entry in groupDict)
            {
                var group = entry.Value.QOpenForRead<Group>();
                if (entityIds.Any(entityId => group.Has(entityId.QOpenForRead<Entity>())))
                {
                    Ungroup(entry.Value);
                    count++;
                }
            }

            return count;
        }

        #endregion

        #region feature

        //public static void Array(Entity entity, string arrayOpt)
        //{
        //}

        //public static void Fillet(Curve cv1, Curve cv2, string bevelOpt)
        //{
        //}

        //public static void Chamfer(Curve cv1, Curve cv2, string bevelOpt)
        //{
        //}

        //public static void Trim(Curve[] baseCurves, Curve cv, Point3d[] p)
        //{
        //}

        //public static void Extend(Curve[] baseCurves, Curve cv, Point3d[] p)
        //{
        //}

        /// <summary>
        /// Sets draworder.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="operation">The operation.</param>
        public static void Draworder(this ObjectId entityId, DraworderOperation operation)
        {
            entityId.Database.BlockTableId.QOpenForWrite<BlockTable>(blockTable =>
            {
                blockTable[BlockTableRecord.ModelSpace].QOpenForWrite<BlockTableRecord>(blockTableRecord =>
                {
                    blockTableRecord.DrawOrderTableId.QOpenForWrite<DrawOrderTable>(drawOrderTable =>
                    {
                        switch (operation)
                        {
                            case DraworderOperation.MoveToTop:
                                drawOrderTable.MoveToTop(new ObjectIdCollection { entityId });
                                break;
                            case DraworderOperation.MoveToBottom:
                                drawOrderTable.MoveToBottom(new ObjectIdCollection { entityId });
                                break;
                            default:
                                break;
                        }
                    });
                });
            });
        }

        /// <summary>
        /// Sets draworder.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="operation">The operation.</param>
        public static void Draworder(this IEnumerable<ObjectId> entityIds, DraworderOperation operation)
        {
            DbHelper
                .GetDatabase(entityIds)
                .BlockTableId
                .QOpenForWrite<BlockTable>(blockTable =>
                {
                    blockTable[BlockTableRecord.ModelSpace].QOpenForWrite<BlockTableRecord>(blockTableRecord =>
                    {
                        blockTableRecord.DrawOrderTableId.QOpenForWrite<DrawOrderTable>(drawOrderTable =>
                        {
                            switch (operation)
                            {
                                case DraworderOperation.MoveToTop:
                                    drawOrderTable.MoveToTop(new ObjectIdCollection(entityIds.ToArray()));
                                    break;
                                case DraworderOperation.MoveToBottom:
                                    drawOrderTable.MoveToBottom(new ObjectIdCollection(entityIds.ToArray()));
                                    break;
                                default:
                                    break;
                            }
                        });
                    });
                });
        }

        #endregion

        #region property

        /// <summary>
        /// Updates a text style.
        /// </summary>
        /// <param name="fontFamily">The font family. e.g."宋体""@宋体"</param>
        /// <param name="textHeight">The text height.</param>
        /// <param name="italicAngle">The italic angle.</param>
        /// <param name="xScale">The X scale.</param>
        /// <param name="vertical">Use vertical.</param>
        /// <param name="bigFont">Use big font.</param>
        /// <param name="textStyleName">The text style name.</param>
        /// <returns>The text style ID.</returns>
        public static ObjectId TextStyle(string fontFamily, double textHeight, double italicAngle = 0, double xScale = 1, bool vertical = false, string bigFont = "", string textStyleName = Consts.TextStyleName)
        {
            var result = DbHelper.GetTextStyleId(textStyleName, true);
            result.QOpenForWrite<TextStyleTableRecord>(tstr =>
            {
                tstr.Font = new FontDescriptor(fontFamily, false, false, 0, 34);
                tstr.TextSize = textHeight;
                tstr.ObliquingAngle = italicAngle;
                tstr.XScale = xScale;
                tstr.IsVertical = vertical;
                tstr.BigFontFileName = bigFont;
            });

            return result;
        }

        /// <summary>
        /// Sets layer.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="layer">The layer name.</param>
        public static void SetLayer(this ObjectId entityId, string layer)
        {
            entityId.QOpenForWrite<Entity>(entity =>
            {
                entity.LayerId = DbHelper.GetLayerId(layer);
            });
        }

        /// <summary>
        /// Sets line type.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="linetype">The line type.</param>
        /// <param name="linetypeScale">The scale.</param>
        public static void SetLinetype(this ObjectId entityId, string linetype, double linetypeScale = 1)
        {
            entityId.QOpenForWrite<Entity>(entity =>
            {
                entity.LinetypeId = DbHelper.GetLinetypeId(linetype);
                entity.LinetypeScale = linetypeScale;
            });
        }

        /// <summary>
        /// Sets dimension style.
        /// </summary>
        /// <param name="dimId">The dimension ID.</param>
        /// <param name="dimstyle">The style name.</param>
        public static void SetDimstyle(this ObjectId dimId, string dimstyle)
        {
            var dimstyleId = DbHelper.GetDimstyleId(dimstyle);
            using (var trans = dimId.Database.TransactionManager.StartTransaction())
            {
                var dim = trans.GetObject(dimId, OpenMode.ForWrite) as Dimension;
                dim.DimensionStyle = dimstyleId;
                trans.Commit();
            }
        }

        /// <summary>
        /// Sets text style for DT, MT, or DIM.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="textStyleName">The text style name.</param>
        public static void SetTextStyle(this ObjectId entityId, string textStyleName)
        {
            var textStyleId = DbHelper.GetTextStyleId(textStyleName);
            entityId.QOpenForWrite<Entity>(entity =>
            {
                if (entity is MText mText)
                {
                    mText.TextStyleId = textStyleId;
                }
                else if (entity is DBText text)
                {
                    text.TextStyleId = textStyleId;
                }
                else if (entity is Dimension dimension)
                {
                    dimension.TextStyleId = textStyleId;
                }
            });
        }

        #endregion
    }
}
