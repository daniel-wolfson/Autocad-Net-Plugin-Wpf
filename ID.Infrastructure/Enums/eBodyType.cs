namespace ID.Infrastructure.Enums
{
    public enum eBodyType
    {
        [DataInfo("Title", "GOV TEXT", -1, 7)]
        Title = 0,

        [DataInfo("Rectangle", "CLOSURE BEZEK", -1, 7)]
        Rectangle = 1,

        [DataInfo("Polyline", "BUILDING PARTNER", 0, 7)]
        Polyline = 2,

        [DataInfo("Donut", "PERFORM", 0, 7)]
        Donut = 3,

        [DataInfo("Circle", "0", 0, 7)]
        Circle = 4,

        [DataInfo("Blockreference", "0", 0, 7)]
        Blockreference = 5,

        [DataInfo("Marker", "0", 0, 7)]
        Marker = 6
    }
}