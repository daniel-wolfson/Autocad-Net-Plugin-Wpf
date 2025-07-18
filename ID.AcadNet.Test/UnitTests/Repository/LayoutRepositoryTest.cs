using System.Collections.Generic;
using System.Linq;
using Intellidesk.Data.Models;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.AcadNet.Test.UnitTests.Fake;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories.EF6.Factories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Repositories.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intellidesk.AcadNet.Test.UnitTests.Repository
{
    [TestClass]
    public class LayouRepositoryTest
    {
        readonly IRepositoryProvider _repositoryProvider = new RepositoryProvider(new RepositoryFactories());

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // todo: delete NorthwindTest.mfd (LocalDb)
            // cleanup all the infrastructure that was needed for our tests.
        }
        
        [TestMethod]
        public void DeleteLayoutById()
        {
            using (var acadNetFakeContext = new AcadNetFakeContext())
            using (IUnitOfWorkAsync unitOfWork = new UnitOfWork(acadNetFakeContext, _repositoryProvider))
            {
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2, ObjectState = ObjectState.Added });

                unitOfWork.SaveChanges();

                unitOfWork.Repository<Layout>().Delete(2);

                unitOfWork.SaveChanges();

                var layout = unitOfWork.Repository<Layout>().Find(2);

                Assert.IsNull(layout);
            }
        }

        [TestMethod]
        public void DeepLoadLayoutWithBlocks()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                //unitOfWork.Repository<BlockDefinition>().Insert(new Block { BlockID = 1, BlockName = "Nokia" });
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2, LayoutName = "Nokia Lumia 1520", ObjectState = ObjectState.Added });

                unitOfWork.SaveChanges();

                var product = unitOfWork.Repository<Layout>().Find(2);

                Assert.IsNotNull(product);
            }
        }

        [TestMethod]
        public void DeleteLayoutByLayout()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2, ObjectState = ObjectState.Added });

                unitOfWork.SaveChanges();

                var layout = unitOfWork.Repository<Layout>().Find(2);

                layout.ObjectState = ObjectState.Deleted;

                unitOfWork.Repository<Layout>().Delete(layout);

                unitOfWork.SaveChanges();

                var layoutDeleted = unitOfWork.Repository<Layout>().Find(2);

                Assert.IsNull(layoutDeleted);
            }
        }

        [TestMethod]
        public void FindLayoutById()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 1 });
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2 });
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 3 });

                unitOfWork.SaveChanges();

                var layout = unitOfWork.Repository<Layout>().Find(2);

                Assert.IsNotNull(layout);
                Assert.AreEqual(2, layout.LayoutID);
            }
        }

        [TestMethod]
        public void GetProductsExecutesQuery()
        {
            using (IDataContextAsync context = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(context, _repositoryProvider))
            {
                var products = unitOfWork.Repository<Layout>().Query().Select().ToList();
                Assert.IsInstanceOfType(products, typeof(List<Layout>));
            }
        }

        [TestMethod]
        public void GetProductsThatHaveBeenDiscontinued()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 1, FSA = false, ObjectState = ObjectState.Added });
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2, FSA = true, ObjectState = ObjectState.Added });
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 3, FSA = true, ObjectState = ObjectState.Added });

                unitOfWork.SaveChanges();

                var discontinuedProducts = unitOfWork.Repository<Layout>().Query(t => t.FSA).Select();

                Assert.AreEqual(2, discontinuedProducts.Count());
            }
        }

        [TestMethod]
        public void InsertProduct()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 1, FSA = false, ObjectState = ObjectState.Added });
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2, FSA = true, ObjectState = ObjectState.Added });
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 3, FSA = true, ObjectState = ObjectState.Added });

                unitOfWork.SaveChanges();

                var product = unitOfWork.Repository<Layout>().Find(2);

                Assert.IsNotNull(product);
                Assert.AreEqual(2, product.LayoutID);
            }
        }

        [TestMethod]
        public void InsertRangeOfProducts()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                var newProducts = new[]
                {
                    new Layout {LayoutID = 1, FSA = false, ObjectState = ObjectState.Added},
                    new Layout {LayoutID = 2, FSA = true, ObjectState = ObjectState.Added},
                    new Layout {LayoutID = 3, FSA = true, ObjectState = ObjectState.Added}
                };

                unitOfWork.Repository<Layout>().InsertRange(newProducts);

                var savedLayouts = unitOfWork.Repository<Layout>().Query().Select();

                Assert.AreEqual(savedLayouts.Count(), newProducts.Length);
            }
        }

        [TestMethod]
        public void UpdateProduct()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2, FSA = true });

                unitOfWork.SaveChanges();

                var layout = unitOfWork.Repository<Layout>().Find(2);

                Assert.AreEqual(layout.FSA, true, "Assert we are able to find the inserted Product.");

                layout.FSA = false;
                layout.ObjectState = ObjectState.Modified;

                unitOfWork.Repository<Layout>().Update(layout);
                unitOfWork.SaveChanges();

                Assert.AreEqual(layout.FSA, false, "Assert that our changes were saved.");
            }
        }

        [TestMethod]
        public async void FindProductKeyAsync()
        {
            using (IDataContextAsync northwindFakeContext = new AcadNetFakeContext())
            using (IUnitOfWorkAsync unitOfWork = new UnitOfWork(northwindFakeContext, _repositoryProvider))
            {
                unitOfWork.Repository<Layout>().Insert(new Layout { LayoutID = 2, FSA = true });

                unitOfWork.SaveChanges();

                var product = await unitOfWork.RepositoryAsync<Layout>().FindAsync(2);

                Assert.AreEqual(product.LayoutID, 2);
            }
        }
    }
}