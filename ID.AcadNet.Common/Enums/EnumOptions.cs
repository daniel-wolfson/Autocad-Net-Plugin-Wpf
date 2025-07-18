namespace Intellidesk.AcadNet.Common.Enums
{
    public enum AlignmentOptions { ByDefault, ByCenterHorizontalMode, ByCenterVerticalMode, ByLeft, ByRigHt }

    public enum PromptBehavior
    {
        EscCancel, EscOrigin, EscContinue
    }

    public enum DocumentOptions
    {
        Active,
        CloseAndSave, CloseAndDiscard,
        Open, Load, OpenAndActive, Save,
        WindowsStateMinimize, WindowsStateMaximize, WindowsStateNormal,
        CloseAll, CloseAllAndSaveAll, SaveAll,
        WindowsStateMinimizeAll, WindowsStateMaximizeAll, WindowsStateNormalAll
    }

    public enum GetPointsOptions { Point, Point2D, Point3D, PointList, Point2DList, Point3DList, DbObject }

    public enum GetSelectOptions { All, AllBlocks, GetPoint, GetObject, GetObjects, None, SelectImplied, SelectLast, SelectWindowPolygon, SelectFromEntityPolygon }

    public enum GetResultOptions { DbObjectColl, EntityColl, ObjectIdColl, XDataValue, ToDisplay }

    public enum UpdateOptions { Color, HighLight, Remove, Rotate, Scale, Displacement }

    public enum ViewOptions { Top, Bottom, Left, Right, Front, Back, Swiso, Seiso, Neiso, Nwiso }
}