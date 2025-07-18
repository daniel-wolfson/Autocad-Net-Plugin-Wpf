using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Common.Enums
{
    public enum eCableType
    {
        [Display(Name = "Add cable type..."), DataInfo("0", 7)]
        NewCable = 0,

        [Display(Name = "12(1x12)"), DataInfo("Cable12(1x12)", 7)]
        Cable12x1x12,

        [Display(Name = "24(1x12)"), DataInfo("Cable24(1x12)", 30)]
        Cable24x1x12,

        [Display(Name = "36(6x6)"), DataInfo("Cable36(6x6)", 1)]
        Cable36x6x6,

        [Display(Name = "48(8x6)"), DataInfo("Cable48(8x6)", 26)]
        Cable48x8x6,

        [Display(Name = "96(16x6)"), DataInfo("Cable96(16x6)", 2)]
        Cable96x16x6,

        [Display(Name = "96(8x12)"), DataInfo("Cable96(8x12)", 2)]
        Cable96x8x12,

        [Display(Name = "96(8x12) 7mm MINI"), DataInfo("Cable96(8x12) 7mm MINI", 2)]
        Cable96x8x12mini,

        [Display(Name = "144(12x12)"), DataInfo("Cable144(12x12)", 160)]
        Cable144x12x12,

        [Display(Name = "144(12x12)7.5mm Turquoise"), DataInfo("Cable144(12x12)7.5mm", 160)]
        Cable144x12x12Turquoise,

        [Display(Name = "288(12x14)"), DataInfo("Cable288(12x14)", 221)]
        Cable288x12x14,

        [Display(Name = "288(12x14) MINIDUST"), DataInfo("Cable288(12x14)", 221)]
        Cable288x12x14Minidust
    }
}