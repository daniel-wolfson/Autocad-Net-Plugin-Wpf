using Intellidesk.AcadNet.Common.Interfaces;
using Unity;

namespace Intellidesk.AcadNet.Core
{
    /// <summary>
    /// This class is instantiated by AutoCAD for each document when
    /// a command is called by the user the first time in the context
    /// of a given document. In other words, non static data in this class
    /// is implicitly per-document!
    /// </summary>

    public class BaseService : IBaseService
    {
        protected readonly IUnityContainer _unityContainer;
        private BaseService(IUnityContainer container)
        {
            _unityContainer = container;
        }
    }
}