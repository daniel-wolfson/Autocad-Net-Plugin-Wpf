using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.Factories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Repositories.Infrastructure;
using Intellidesk.AcadNet.Test.UnitTests.Fake;
using Intellidesk.Data.Models.Cad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intellidesk.AcadNet.Test.IntegrationTests
{
    [TestClass]
    public class LayoutRepositoryTest
    {
        private static readonly string MasterConnectionString = 
            ConfigurationManager.ConnectionStrings["IntelliDesktopContext"].ConnectionString;
        private readonly IRepositoryProvider _repositoryProvider = 
            new RepositoryProvider(new RepositoryFactories());

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SettingUpNorthwindTestDatabase()
        {
            //TestContext.WriteLine("Please ensure Northwind.Test/Sql/instnwnd.sql is copied to C:\\temp\\instnwnd.sql for test to run succesfully");
            //TestContext.WriteLine("Please verify the the Northwind.Test/app.config connection strings are correct for your environment");

            //TestContext.WriteLine("TestFixture executing, creating NorthwindTest Db for integration  tests");
            //TestContext.WriteLine("Loading and parsing create NorthwindTest database Sql script");

            //var file = new FileInfo("C:\\temp\\instnwnd.sql");
            //var script = file.OpenText().ReadToEnd();
            ////RunSqlOnMaster(script);
            //TestContext.WriteLine("NorthwindTest Db created for integration tests");
        }

        private static void RunSqlOnMaster(string script)
        {
            using (var connection = new SqlConnection(MasterConnectionString))
            {
                //var server = new Server(new ServerConnection(connection));
                //server.ConnectionContext.ExecuteNonQuery(script);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void InsertProducts()
        {
            using (IDataContextAsync context = new AcadNetFakeContext())
            using (IUnitOfWorkAsync unitOfWork = new UnitOfWork(context, _repositoryProvider))
            {
                IRepositoryAsync<Layout> layoutRepository = new Repository<Layout>(context, unitOfWork);

                var l = new Layout() { LayoutID = (decimal)0, LayoutName = "One", FSA = false };

                var newProducts = new[]
                {
                    new Layout {LayoutID = 0, LayoutName = "One", FSA = false, ObjectState = ObjectState.Added},
                    new Layout {LayoutName = "12345678901234567890123456789012345678901234567890", FSA = true, ObjectState = ObjectState.Added},
                    new Layout {LayoutName = "Three", FSA = true, ObjectState = ObjectState.Added},
                    new Layout {LayoutName = "Four", FSA = true, ObjectState = ObjectState.Added},
                    new Layout {LayoutName = "Five", FSA = true, ObjectState = ObjectState.Added}
                };

                foreach (var product in newProducts)
                {
                    try
                    {
                        layoutRepository.Insert(product);
                        unitOfWork.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var sb = new StringBuilder();

                        foreach (var failure in ex.EntityValidationErrors)
                        {
                            sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());

                            foreach (var error in failure.ValidationErrors)
                            {
                                sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                                sb.AppendLine();
                            }
                        }

                        Debug.WriteLine(sb.ToString());
                        TestContext.WriteLine(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        TestContext.WriteLine(ex.Message);
                    }
                }

                var insertedProduct = layoutRepository.Query(x => x.LayoutName == "One").Select().FirstOrDefault();
                Assert.IsTrue(insertedProduct.LayoutName == "One");
            }
        }
    }
}
