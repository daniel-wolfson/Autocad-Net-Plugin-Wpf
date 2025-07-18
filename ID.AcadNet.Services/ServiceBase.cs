using Unity;

namespace Intellidesk.AcadNet.Services
{
    public class ServiceBase<T> where T : class
    {
        [Dependency("ContainerAccessor")]
        protected IUnityContainer ServiceUnityContainer { get; set; }

        public ServiceBase()
        {
            //container = new UnityContainer();
            //container.Resolve<Bootstrapper>()
            //    .RegisterModule(typeof(AcadNetPlugin))
            //    .RegisterModule(typeof(UIBuildService));
            //ContainerBootStrapper.RegisterTypes(container);

            //AcadNetData = ContainerAccessor.Resolve<IDataContextAsync>("AcadNetContext");
            InjectDependencies();
        }

        protected virtual void InjectDependencies()
        {
            ServiceUnityContainer = new UnityContainer();
            ServiceUnityContainer.BuildUp(this as T);
        }
    }
}