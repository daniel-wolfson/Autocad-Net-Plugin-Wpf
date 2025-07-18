using System;
using System.Reflection;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Models.DataContext;
using Intellidesk.Data.Repositories.EF6.Factories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intellidesk.AcadNet.Test.IntegrationTests
{
    [TestClass]
    public class UnitOfWorkTests
    {
        [TestMethod]
        public void UnitOfWork_Transaction_Test()
        {
            IRepositoryProvider repositoryProvider = new RepositoryProvider(new RepositoryFactories());

            using (IDataContextAsync context = new AcadNetContext())
            using (IUnitOfWorkAsync unitOfWork = new UnitOfWork(context, repositoryProvider))
            {
                IRepositoryAsync<Layout> layoutRepository = new Repository<Layout>(context, unitOfWork);
                //IPluginManager pluginManager = new PluginManager();
                //IService<Layout> layoutService = new LayoutService(unitOfWork, pluginManager);

                try
                {
                    unitOfWork.BeginTransaction();

                    //layoutService.Insert(new Layout { LayoutId = 0, LayoutName = "LayoutSample1", ObjectState = ObjectState.Added });
                    //layoutService.Insert(new Layout { LayoutId = 1, LayoutName = "LayoutSample2", ObjectState = ObjectState.Added });

                    //var customer = layoutService.Find("LayoutSample1");
                    //Assert.AreSame(customer.LayoutId, "LayoutSample1");

                    //customer = layoutService.Find("LayoutSample2");
                    //Assert.AreSame(customer.LayoutId, "LayoutSample2");

                    // save
                    var saveChangesAsync = unitOfWork.SaveChanges();
                    //Assert.AreSame(saveChangesAsync, 2);

                    // Will cause an exception, cannot insert customer with the same CustomerId (primary key constraint)
                    //layoutService.Insert(new Layout { LayoutId = 0, LayoutName = "SkyRanch", ObjectState = ObjectState.Added });
                    //save 
                    unitOfWork.SaveChanges();

                    unitOfWork.Commit();
                }
                catch (Exception e)
                {
                    unitOfWork.Rollback();
                }
            }
        }

        [TestMethod]
        public void UnitOfWork_Dispose_Test()
        {
            IRepositoryProvider repositoryProvider = new RepositoryProvider(new RepositoryFactories());
            IDataContextAsync context = new AcadNetContext();
            IUnitOfWorkAsync unitOfWork = new UnitOfWork(context, repositoryProvider);

            // opening connection
            unitOfWork.BeginTransaction();
            unitOfWork.Commit();

            // calling dispose 1st time
            unitOfWork.Dispose();
            var isDisposed = (bool) GetInstanceField(typeof (UnitOfWork), unitOfWork, "_disposed");
            Assert.IsTrue(isDisposed);

            // calling dispose 2nd time, should not throw any excpetions
            unitOfWork.Dispose();
            context.Dispose();

            // calling dispose 3rd time, should not throw any excpetions
            context.Dispose();
            unitOfWork.Dispose();
        }

        [TestMethod]
        public void IDataContext_Dispose_Test()
        {
            IRepositoryProvider repositoryProvider = new RepositoryProvider(new RepositoryFactories());
            IDataContextAsync context = new AcadNetContext();
            IUnitOfWorkAsync unitOfWork = new UnitOfWork(context, repositoryProvider);

            // opening connection
            unitOfWork.BeginTransaction();
            unitOfWork.Commit();

            // calling dispose 1st time
            context.Dispose();

            var isDisposed = (bool) GetInstanceField(typeof (DataContext), context, "_disposed");
            Assert.IsTrue(isDisposed);

            // calling dispose 2nd time, should not throw any excpetions
            unitOfWork.Dispose();
            context.Dispose();

            // calling dispose 3rd time, should not throw any excpetions
            unitOfWork.Dispose();
            context.Dispose();
        }

        private static object GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field != null ? field.GetValue(instance) : null;
        }

        public TestContext TestContext { get; set; }
    }
}