using System;
using System.ComponentModel;

namespace Intellidesk.AcadNet.Common.Enums
{
    public enum ProjectTypedValues
    {
        [Description("Israeli Transverse Mercator")]
        CoordSystem = 1024,
        [Description("Percentage")]
        Percentage = 1025,
        [Description("LastExtents X Min")]
        LastExtentsXMin = 1026,
        [Description("LastExtents Y Min")]
        LastExtentsYMin = 1027,
        [Description("LastExtents X Min")]
        LastExtentsXMax = 1028,
        [Description("LastExtents Y Min")]
        LastExtentsYMax = 1029,
    }

    /// <summary>Common Task Dialog icons. Determine the look of the main instruction.</summary>
    public enum TaskDialogIconWin7 : long
    {
        Information = UInt16.MaxValue - 2,
        Warning = UInt16.MaxValue,
        Stop = UInt16.MaxValue - 1,
        None = 0,
        SecurityWarning = UInt16.MaxValue - 5,
        SecurityError = UInt16.MaxValue - 6,
        SecuritySuccess = UInt16.MaxValue - 7,
        SecurityShield = UInt16.MaxValue - 3,
        SecurityShieldBlue = UInt16.MaxValue - 4,
        SecurityShieldGray = UInt16.MaxValue - 8
    }

    public enum EntityTypes { DBTEXT, CIRCLE, LINE, LWPOLYLINE, POLYLINE, POLYLINE2D, POLYLINE3D, TEXT, MTEXT, CURVE }

    public enum LineTypes { Continuous, DASHED, Center, DOT, HIDDEN2 }

    public enum MsgMode { Save = 100, Display = 101, TestMode = 102, Ok = 255, Err = 0, Warning = 2, SentToPrint = 3, SentToEmail = 4 }

    public enum PaletteState { ToBeClosed, ToBeLoaded, Show, Hide, Docked, Free, Init, Loaded, ToBeClose };

    public enum XDataTypeCode { App = 1001, Data = 1000, CntId = 0, TypeId = 1, FloorId = 2, Rotate = 3, Placement = 4, In1 = 5, Out1 = 6 }

    public enum CoordSystem
    {
        ITM, WGS84
    }
}