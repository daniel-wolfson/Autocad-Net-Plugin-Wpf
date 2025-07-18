using System;

namespace Intellidesk.AcadNet
{
    public interface ICommandClassService : IDisposable
    {
        void Start();

        void Dispose(bool flag);
    }
}