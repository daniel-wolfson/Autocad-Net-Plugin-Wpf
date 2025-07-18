namespace Intellidesk.AcadNet.Common
{
    /// <summary>
    /// Draworder type.
    /// </summary>
    public enum DraworderOperation
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None = 0,
        /// <summary>
        /// Move above.
        /// </summary>
        MoveAbove = 1,
        /// <summary>
        /// Move below.
        /// </summary>
        MoveBelow = 2,
        /// <summary>
        /// Move to top.
        /// </summary>
        MoveToTop = 3,
        /// <summary>
        /// Move to bottom.
        /// </summary>
        MoveToBottom = 4
    }

    /// <summary>
    /// Boundary trace entity type.
    /// </summary>
    public enum BoundaryType
    {
        /// <summary>
        /// Trace with polyline.
        /// </summary>
        Polyline = 0,
        /// <summary>
        /// Trace with region.
        /// </summary>
        Region = 1
    }
}
