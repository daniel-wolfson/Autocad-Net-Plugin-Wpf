using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Intellidesk.AcadNet.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Internal
{
    /// <summary>
    /// Quick selection toolbox.
    /// </summary>
    public static class QuickSelection
    {
        // TODO: remove all obsolete methods.

        #region QWhere|QPick|QSelect

        /// <summary>
        /// QLinq Where.
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>Filtered IDs.</returns>
        public static IEnumerable<ObjectId> XWhere(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            return ids
                .XOpenForRead<Entity>()
                .Where(filter)
                .Select(entity => entity.ObjectId);
        }

        [Obsolete("Use QOpenForRead().")]
        public static ObjectId XPick(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            // TODO: verify that the default works.
            return ids
                .XOpenForRead<Entity>()
                .FirstOrDefault()
                .ObjectId;
        }

        /// <summary>
        /// QLinq Select.
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns>Mapped results.</returns>
        public static IEnumerable<TResult> XSelect<TResult>(this IEnumerable<ObjectId> ids, Func<Entity, TResult> mapper)
        {
            return ids
                .XOpenForRead<Entity>()
                .Select(mapper);
        }

        [Obsolete("Use QOpenForRead().")]
        public static TResult XSelect<TResult>(this ObjectId entId, Func<Entity, TResult> mapper)
        {
            var ids = new List<ObjectId> { entId };
            return ids.XSelect(mapper).First();
        }

        #endregion

        #region Aggregation: QCount|QMin|QMax

        [Obsolete("Use QOpenForRead().")]
        public static int XCount(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            return ids
                .XOpenForRead<Entity>()
                .Count(filter);
        }

        [Obsolete("Use QOpenForRead().")]
        public static double XMin(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids
                .XOpenForRead<Entity>()
                .Min(mapper);
        }

        [Obsolete("Use QOpenForRead().")]
        public static double XMax(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids
                .XOpenForRead<Entity>()
                .Max(mapper);
        }

        [Obsolete("Use QOpenForRead().")]
        public static ObjectId XMinEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            // Bad implementation.
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartOpenCloseTransaction())
            {
                var entities = ids.Select(id => trans.GetObject(id, OpenMode.ForRead) as Entity).ToList();
                double value = entities.Min(mapper);
                return entities.First(entity => mapper(entity) == value).ObjectId;
            }
        }

        [Obsolete("Use QOpenForRead().")]
        public static ObjectId XMaxEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            // Bad implementation.
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartOpenCloseTransaction())
            {
                var entities = ids.Select(id => trans.GetObject(id, OpenMode.ForRead) as Entity).ToList();
                double value = entities.Max(mapper);
                return entities.First(entity => mapper(entity) == value).ObjectId;
            }
        }

        #endregion

        #region Factory methods

        /// <summary>
        /// Selects all entities with specified DXF type in current editor.
        /// </summary>
        /// <param name="dxfType">The DXF type.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] SelectAll(string dxfType)
        {
            return XSelectAll(new TypedValue((int)DxfCode.Start, dxfType));
        }

        /// <summary>
        /// Selects all entities with specified filters in current editor.
        /// </summary>
        /// <param name="filterList">The filter list.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] SelectAll(FilterList filterList)
        {
            return XSelectAll(filterList.ToArray());
        }

        /// <summary>
        /// Selects all entities with specified filters in current editor.
        /// </summary>
        /// <param name="filterList">The filter list.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] XSelectAll(params TypedValue[] filterList)
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var selRes = filterList != null && filterList.Any()
                ? ed.SelectAll(new SelectionFilter(filterList))
                : ed.SelectAll();

            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds();
            }

            return new ObjectId[0];
        }

        private static IEnumerable<ObjectId> XSelectAllInternal(this Database db, string block)
        {
            using (var trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                var bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace = trans.GetObject(bt[block], OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId id in modelSpace)
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// Selects all entities in specified database's model space.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] XSelectAll(this Database db)
        {
            return db.XSelectAllInternal(BlockTableRecord.ModelSpace).ToArray();
        }

        /// <summary>
        /// Selects all entities in specified database's specified block.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="block">The block.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] XSelectAll(this Database db, string block)
        {
            return db.XSelectAllInternal(block).ToArray();
        }

        #endregion
    }

    /// <summary>
    /// Filter list helper.
    /// </summary>
    public class FilterList
    {
        private readonly List<TypedValue> Cache = new List<TypedValue>();

        /// <summary>
        /// Creates a new filter list.
        /// </summary>
        /// <returns>The result.</returns>
        public static FilterList Create()
        {
            return new FilterList();
        }

        /// <summary>
        /// Gets the TypedValue array representation.
        /// </summary>
        /// <returns>The array.</returns>
        public TypedValue[] ToArray()
        {
            return Cache.ToArray();
        }

        /// <summary>
        /// Adds a DXF type filter.
        /// </summary>
        /// <param name="dxfTypes">The DXF types.</param>
        /// <returns>The helper.</returns>
        public FilterList DxfType(params string[] dxfTypes)
        {
            Cache.Add(new TypedValue((int)DxfCode.Start, string.Join(",", dxfTypes)));
            return this;
        }

        /// <summary>
        /// Adds a layer filter.
        /// </summary>
        /// <param name="layers">The layers.</param>
        /// <returns>The helper.</returns>
        public FilterList Layer(params string[] layers)
        {
            Cache.Add(new TypedValue((int)DxfCode.LayerName, string.Join(",", layers)));
            return this;
        }

        /// <summary>
        /// Adds an arbitrary filter.
        /// </summary>
        /// <param name="typeCode">The type code.</param>
        /// <param name="value">The value.</param>
        /// <returns>The helper.</returns>
        public FilterList Filter(int typeCode, string value)
        {
            Cache.Add(new TypedValue(typeCode, value));
            return this;
        }
    }
}
