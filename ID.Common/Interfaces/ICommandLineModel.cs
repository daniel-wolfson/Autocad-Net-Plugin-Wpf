namespace Intellidesk.Common.Interfaces
{
    public interface ICommandLineModel
    {
        void Execute(string command, object parameter = null);
    }
}