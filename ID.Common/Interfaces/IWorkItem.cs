namespace Intellidesk.Interfaces
{
    public interface IWorkItem
    {
        string FullPath { get; set; }
        string Work { get; set; }
        string WorkTypeName { get; set; }
    }
}