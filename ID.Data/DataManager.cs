using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Intellidesk.Infrastructure;
using Intellidesk.NetTools;

namespace Intellidesk.AcadNet.Data
{
    public static partial class DataManager
    {
        public static DbContext Context;

        static DataManager()
        {
            //Context.Configuration.AutoDetectChangesEnabled = false;
            //Context.Database.Initialize(true); 
        }

        //public static partial class DataManager public class DataManager<T> : DbContext where T : DbContext
        //protected static T Context;
        //public static T CreateContext(string exePath = "", string connectionString = "")
        //{
        //    //Search connection string: "configFileFullPath" or  common Config from DataTools.dll.config
        //    var config = ConfigurationManager.OpenExeConfiguration(String.IsNullOrEmpty(exePath) ? Assembly.GetExecutingAssembly().Location : exePath);
        //    if (config.HasFile)
        //    {
        //        var settings = config.ConnectionStrings.ConnectionStrings[String.IsNullOrEmpty(connectionString) ? typeof(T).Name : connectionString];
        //        if (settings != null)
        //            Context = (T)Activator.CreateInstance(typeof(T), settings.ConnectionString);
        //    }
        //    return Context;
        //}

        //public static void CreateService<T>(string exeConfigFilename = "", string connectionNameOrString = "") where T : DbContext
        //{
        //}

        /// <summary>Create new context (Entity data model)
        /// <param name="exeConfigFilename">,where [exeConfigFilename] is path to assembly or location of config file</param>
        /// <param name="connectionNameOrString">, [connectionNameOrString] is name of section or connectionString.</param>
        /// </summary>
        public static T CreateContext<T>(string exeConfigFilename = "", string connectionNameOrString = "", T contextType = null) where T : DbContext
        {
            var connectionString = GetConnectionString<T>(exeConfigFilename, connectionNameOrString);
            Context = Activator.CreateInstance(typeof(T), connectionString) as T;
            return (T)Context;
        }

        //Get Connection String
        public static string GetConnectionString<T>(string exeConfigFilename = "", string connectionNameOrString = "")
        {
            Configuration config;
            if (exeConfigFilename != "")
            {
                var configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = exeConfigFilename };
                config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            }
            else
            {
                var localPath = new Uri(typeof(T).Assembly.CodeBase).LocalPath;
                //Net creates Cache for assembly(see, AppDomain.CurrentDomain.SetupInformation.CachePath) and CachePath != typeof(T).Assembly.Location 
                //therefore here used LocalPath
                config = ConfigurationManager.OpenExeConfiguration(localPath);
            }

            if (config.HasFile)
            {
                if (!connectionNameOrString.Contains("connectionString"))
                {
                    var settings = config.ConnectionStrings.ConnectionStrings[String.IsNullOrEmpty(connectionNameOrString) ? typeof(T).Name : connectionNameOrString];
                    if (settings != null)
                    {
                        connectionNameOrString = settings.ConnectionString;
                    }
                }
            }
            return connectionNameOrString;
        }

        /// <summary> XGetObjectContext converts context to type ObjectContext </summary>
        public static ObjectContext XGetObjectContext(this DbContext context)
        {
            return (((IObjectContextAdapter)context).ObjectContext);
        }

        /// <summary>(TEST!!!) XAdd is expression method for inherited type DbSet
        /// <param name="exp">, where lambda is expression intilization</param>
        /// <returns>and returns new object inherited from DbContext (TEST!!!)</returns>
        /// </summary>
        public static TEntity XAdd<TEntity>(this DbSet<TEntity> dbset, Expression<Func<TEntity, object>> exp) where TEntity : class
        {
            var entity = Activator.CreateInstance<TEntity>();
            //context.Set<TEntity>().Add(entity); //for save changes necessary context.XSaveChanges(); 
            return entity;
        }

        /// <summary> XRemove marking entity for delete by single or multyple primary keys
        ///<para>example: <example>db.LO_Blocks.XRemove(volue1,volue2)</example>, where Key1 = 1, Key2 = 2</para>
        /// </summary>
        public static void XRemove<TEntity>(this DbSet<TEntity> dbset, params object[] keyValues) where TEntity : class
        {
            var ent = dbset.Find(keyValues);
            if (ent != null) dbset.Remove(ent);
        }

        /// <summary>XRemoveFor marks entity for delete.
        ///<example>example: db.LO_Blocks.XRemoveFor(x => x.ID == 0);</example>
        /// </summary>
        public static void XRemoveFor<TEntity>(this DbSet<TEntity> dbset, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            try
            {
                var data = dbset.Where(predicate);
                foreach (var entity in data)
                {
                    dbset.Remove(entity);
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }

        }

        public static string GetEntities<TEntity>(this DbSet<TEntity> dbset, params object[] pars) where TEntity : class
        {
            var result = "";
            foreach (var par in pars.ToList())
            {
                var t = par;
                var info = typeof(TEntity).GetProperties().Where(p => p.Name == Convert.ToString(t));
                var names = info.ToArray();
                info.FirstOrDefault().GetValue(dbset, new object[] { });
            }
            //Console.WriteLine(info.FirstOrDefault().Name);
            return result;
        }

        /// <summary>Update Entity</summary>
        public static TEntity XUpdate<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            return entity;
        }

        /// <summary> (TEST!!! XUpdateEntity) </summary>
        public static TEntity XUpdateEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            var tracked = context.Set<TEntity>().Find(context.XGetKeyValuesFor(entity));
            if (tracked != null)
            {
                context.Entry(tracked).CurrentValues.SetValues(entity);
                return tracked;
            }
            return entity;
        }

        #region "Store procedures, Queries"

        /// <summary> Makes a SqlCommand by the generic type data </summary>
        public static int MakeBatchSqlCommand<T>(ITaskArgs taskArgs, string commandPattern, List<T> data) where T : class
        {
            return MakeBatchSqlCommandFor<T>(taskArgs, commandPattern, data);
        }

        /// <summary> Makes a command by the generic type data with dependency by filterNames, filterValues </summary>
        public static int MakeBatchSqlCommandFor<T>(ITaskArgs taskArgs, string commandPattern, List<T> data,
                                                 string[] filterNames = null, string[] filterValues = null) where T : class
        {
            var recCount = 0;
            try
            {
                if (!data.Any()) return 0;

                //creates buffer to result query
                var buffQuery = new StringBuilder(50000);

                for (var i = 0; i < data.Count(); i++)
                {
                    var props = new Dictionary<string, object>();
                    var i1 = i;
                    var propInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(x => x.PropertyType.Namespace == "System" && x.GetValue(data[i1], null) != null);

                    propInfo.Select(x => x).ToList().ForEach(x => props.Add(x.Name, x.GetValue(data[i], null).ToString() ));

                    var names = string.Join(",", props.Select(x => x.Key));
                    var values = string.Join(",", props.Select(x => (x.Value is string)
                                                                        ? String.Format("'{0}'", x.Value.ToString().Replace("'", "!")) 
                                                                        : x.Value));

                    //dependency filter on names
                    var skip = false;
                    if (filterNames != null && filterNames.Length > 0)
                    {
                        if (!filterNames.Any(names.Contains)) skip = true;
                    }
                    
                    // filter names
                    if (!skip && filterValues != null && filterValues.Length > 0)
                    {
                        if (!filterValues.Any(values.Contains)) skip = true;
                    }

                    if (!skip)
                        buffQuery.Append(String.Format(commandPattern, typeof(T).Name + "s", names, values)); //.Replace(", )", ")")

                    if (i > 0 && i % taskArgs.BatchSize == 0 && buffQuery.Length > 0)
                    {
                        recCount += taskArgs.Context.Database.ExecuteSqlCommand(buffQuery.ToString());
                        buffQuery.Clear();
                    }
                    props.Clear();
                }

                if (buffQuery.Length > 0)
                {
                    recCount += taskArgs.Context.Database.ExecuteSqlCommand(buffQuery.ToString());
                    buffQuery.Clear();
                }
            }

            catch (Exception ex)
            {
                var msg = ex.InnerException == null ? ex.Message : "";
                msg = ex.InnerException != null && ex.InnerException.InnerException == null ? msg : "";
                taskArgs.ErrorInfo.Add(msg);
                taskArgs.Status = StatusOptions.Error;
                //Log.Add(ex);
            }

            // add info about current action into dialog window 
            taskArgs.ExpandedInfo += String.Format("{0} ({1});", typeof(T).Name, recCount);
            return recCount;
        }

        /// <summary>Executes the given DDL/DML command against the database</summary>
        public static void XExecuteSqlCommand(this DbContext context, string sql, params object[] parameters)
        {
            context.Database.ExecuteSqlCommand(sql, parameters);
        }

        public static string XFormatSqlInsertCommand(this DbContext context, string tableName, string columnsNames, params object[] pars)
        {
            int i;
            var result = "INSERT INTO ";
            result = result + tableName + "(" + columnsNames + ") VALUES (";

            for (i = 0; i <= pars.Length - 1; i++)
            {
                string currentValue = "";
                if (pars[i] is string)
                    currentValue = "'" + Convert.ToString(pars[i]) + "'";
                else
                    currentValue = Convert.ToString(pars[i]);
                if (i < pars.Length - 1)
                    currentValue = currentValue + ",";
                result = result + currentValue;
            }

            result = result + ")";
            return result;
        }


        /// <summary>(TEST!!!) XSqlQuery executing sqlCommand.
        /// <example><para>example: context.Database.XSqlQuery&lt;int&gt;("SELECT COUNT(*) FROM Employees");</para></example>
        /// <example><para>example: context.Database.XSqlQuery("SELECT * FROM Employees");</para></example>
        /// <example><para>example: context.Database.SqlQuery&lt;TEntity&gt;("mySpName \@p0, \@p1, \@p2", param1, param2, param3)</para></example>
        /// <example><para><code>example: context.Database.SqlQuery&lt;TEntity&gt;("mySpName {0}, {1}, {2}", param1, param2, param3).ToList();</code></para></example>
        /// <example><para><code>example: context.Database.SqlQuery&lt;TEntity&gt;("EXEC ProcName @param1, @param2", new SqlParameter("param1", param1), new SqlParameter("param2", param2));</code></para></example>
        /// </summary>
        public static IEnumerable<T> XSqlQuery<T>(this DbContext context, string sql, params object[] parameters)
        {
            //returned entity type doesn't have to be represented by ObjectSet/DBSet
            IEnumerable<T> result = null;
            try
            {
                result = context.Database.SqlQuery<T>(sql, parameters); //new SqlParameter("param1", param1)
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }

            return result;
        }

        public static bool XSqlQuery(this DbContext context, string sql, params object[] parameters)
        {
            //returned entity type doesn't have to be represented by ObjectSet/DBSet
            var result = false;
            try
            {
                context.Database.SqlQuery(null, sql, parameters); //new SqlParameter("param1", param1)
                result = true;
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            return result;
        }

        /// <summary>(TEST!!!) XSqlQuery executing sqlCommand.
        /// <example>example: <para>db.XSqlQuery("SELECT COUNT(*)");</para></example>
        /// <example>example: <para>db.XSqlQuery("SELECT *");</para></example>
        /// <example>example: <para>db.XSqlQuery("mySpName");</para></example>
        /// <returns>returned entity type doesn't have to be represented by ObjectSet/DBSet</returns>
        /// </summary>
        public static DbSqlQuery<TEntity> XSqlQuery<TEntity>(this DbSet<TEntity> dbset, string sql, params object[] parameters) where TEntity : class
        {
            return dbset.SqlQuery(sql, parameters);
        }

        public static DataSet XConvertToDataSet<T>(this IEnumerable<T> varList)
        {
            var ds = new DataSet();

            if (varList == null) return ds;
            ds.Tables.Add(varList.XConvertToDataTable());
            return ds;
        }

        public static DataTable XConvertToDataTable<T>(this IEnumerable<T> varlist)
        {
            var dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow 
                if (oProps == null)
                {
                    oProps = rec.GetType().GetProperties();
                    foreach (var pi in oProps)
                    {
                        var colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                var dr = dtReturn.NewRow();

                foreach (var pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) ?? DBNull.Value;
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        public static DataTable XConvertToDataTable(this DataContext context, IQueryable query)
        {
            if (query == null) throw new ArgumentNullException("query");

            IDbCommand cmd = context.GetCommand(query);
            var adapter = new SqlDataAdapter { SelectCommand = (SqlCommand)cmd };
            var dt = new DataTable("sd");

            try
            {
                cmd.Connection.Open();
                adapter.FillSchema(dt, SchemaType.Source);
                adapter.Fill(dt);
            }
            finally
            {
                cmd.Connection.Close();
            }
            return dt;
        }

        public static DataSet XExecuteStoredProcedure(this ObjectContext context, string storedProcedureName, IEnumerable<SqlParameter> parameters)
        {
            var entityConnection = (EntityConnection)context.Connection;
            var conn = entityConnection.StoreConnection;
            var initialState = conn.State;

            var dataSet = new DataSet();

            try
            {
                if (initialState != ConnectionState.Open)
                    conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = storedProcedureName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            var dt = new DataTable();
                            dt.Load(reader);
                            dataSet.Tables.Add(dt);
                        }
                        while (reader.NextResult());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            finally
            {
                if (initialState != ConnectionState.Open)
                    conn.Close();
            }
            return dataSet;
        }

        #endregion

        #region "Keys entity methods"

        public static IEnumerable<object> XGetKeyValuesFor(this DbContext context, object entity)
        {
            Contract.Requires(context != null);
            Contract.Requires(entity != null);

            var entry = context.Entry(entity);
            return context.XGetKeyNamesFor(entity.GetType()).Select(k => entry.Property(k).CurrentValue);
        }
        public static IEnumerable<string> XGetKeyNamesFor(this DbContext context, Type type)
        {
            Contract.Requires<ArgumentNullException>(context != null, "This context don't have to be Null");
            Contract.Requires<ArgumentNullException>(type != null, "This TEntity don't have to be Null");

            type = ObjectContext.GetObjectType(type);

            var metadataWorkspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var objectItemCollection = (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);

            var ospase = metadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);
            if (ospase != null)
            {
                var ospaceType = ospase.SingleOrDefault(t => objectItemCollection.GetClrType(t) == type);
                if (ospaceType == null)
                {
                    throw new ArgumentException(
                        string.Format("The type '{0}' is not mapped as an entity type.", type.Name), "type");
                }

                return ospaceType.KeyMembers.Select(k => k.Name);
            }
            return null;
        }
        private static IEnumerable<object> XGetKeyValuesFor<TEntity>(TEntity entity) where TEntity : class
        {
            return typeof(TEntity).GetProperties()
                                  .Where(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Length != 0)
                                  .Select(k => k.GetValue(entity, null));
        }
        public static string[] XGetKeyNamesFor<TEntity>(this DbContext context) where TEntity : EntityObject
        {
            //if (context == null) throw new ArgumentNullException("context");
            var entitySet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<TEntity>().EntitySet;
            return entitySet.ElementType.KeyMembers.Select(k => k.Name).ToArray();
        }
        public static System.Data.Entity.Core.EntityKey XGetKey(this DbContext context, EntityObject entity)
        {
            return ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager.GetObjectStateEntry(entity).EntityKey;
        }
        public static IEnumerable<string> XGetKeys<TEntity>(this DbContext context) where TEntity : EntityObject
        {
            //if (context == null) throw new ArgumentNullException("context");
            var entitySet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<TEntity>().EntitySet;
            return entitySet.ElementType.KeyMembers.Select(x => x.Name);
        }

        #endregion

        public static DbSet<T> XGetDbSet<T>(this DbContext context) where T : class
        {
            DbSet<T> retVal = null;
            var info = context.GetType().GetProperties().FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>));
            if (info != null) retVal = (DbSet<T>)info.GetValue(context, null);
            return retVal;
        }
        
        public static string XGetDbSetName<T>(this DbContext context) where T : class
        {
            var retVal = "";
            var info = context.GetType().GetProperties().FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>));
            if (info != null) retVal = info.Name;
            return retVal;
        }

        public static T XGetById<T>(this DbContext context, object id) where T : EntityObject
        {
            return context.XGetObjectContext().CreateObjectSet<T>()
                          .SingleOrDefault(t => t.EntityKey.EntityKeyValues[0].Value == id);
        }

        public static IEnumerable<object> GetNamedDbSet(this DbContext context, string dbSetName)
        {
            var property = context.GetType().GetProperty(dbSetName);
            if (property == null || !property.CanRead)
            {
                throw new ArgumentException("DbSet named " + dbSetName + " does not exist.");
            }

            var result = new List<object>();
            foreach (var item in (IEnumerable)property)
            {
                result.Add(item);
            }
            return result;
        }

        private static int GetMaxID<TEntity>(this DbContext context, Expression<Func<TEntity, int?>> expression) where TEntity : class
        {
            return context.Set<TEntity>().Max(expression) ?? 0;
        }

        /// <summary>Expression method for inherited type DbContext </summary>
        private static void XSaveChanges(this DbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException validationException)
            {
                foreach (var error in validationException.EntityValidationErrors)
                {
                    var entry = error.Entry;
                    foreach (var err in error.ValidationErrors)
                    {
                        Debug.WriteLine(err.PropertyName + " " + err.ErrorMessage);
                    }
                }
            }
            catch (DbUpdateConcurrencyException concurrencyException)
            {
                //assume just one
                var dbEntityEntry = concurrencyException.Entries.First();
                //store wins
                dbEntityEntry.Reload();
                //OR client wins
                var dbPropertyValues = dbEntityEntry.GetDatabaseValues();
                dbEntityEntry.OriginalValues.SetValues(dbPropertyValues); //orig = db
            }
            catch (DbUpdateException updateException)
            {
                //often in innerException
                if (updateException.InnerException != null)
                    Debug.WriteLine(updateException.InnerException.Message);
                //which exceptions does it relate to
                foreach (var entry in updateException.Entries)
                {
                    Debug.WriteLine(entry.Entity);
                }
            }


        }

        public static string GetSqlCeConnectionString<T>(string fileName, string edmxFileName) where T : class
        {
            var csBuilder = new EntityConnectionStringBuilder
                {
                    Provider = "System.Data.SqlServerCe.3.5",
                    ProviderConnectionString = string.Format("Data Source={0};", fileName),
                    Metadata = string.Format("res://{0}/{1}.csdl|res://{0}/{1}.ssdl|res://{0}/{1}.msl",
                                             typeof(T).Assembly.FullName, edmxFileName)
                };
            return csBuilder.ToString();
        }

        public static string GetSqlConnectionString<T>(string serverName, string databaseName, string edmxFileName) where T : class
        {
            var providerCs = new SqlConnectionStringBuilder
                {
                    DataSource = serverName,
                    InitialCatalog = databaseName,
                    IntegratedSecurity = true
                };

            var csBuilder = new EntityConnectionStringBuilder
                {
                    Provider = "System.Data.SqlClient",
                    ProviderConnectionString = providerCs.ToString(),
                    Metadata = string.Format("res://{0}/{1}.csdl|res://{0}/{1}.ssdl|res://{0}/{1}.msl",
                                             typeof(T).Assembly.FullName, edmxFileName)
                };
            return csBuilder.ToString();
        }

        public static T FetchResult<T>(this ObjectResult<T> result)
        {
            return (T)result.First();
        }

        public static IEnumerable<T> WhereLike<T>(this IEnumerable<T> data,
               string propertyOrFieldName, string value, string callMethodName, Type callClass)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.Call(
                callClass.GetMethod(callMethodName,
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public),
                    Expression.PropertyOrField(param, propertyOrFieldName),
                    Expression.Constant(value, typeof(string)));
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return data.Where(lambda.Compile());
        }

        public static IEnumerable<T> WhereFilter<T>(this IEnumerable<T> data,
               string propertyOrFieldName, object value)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.Equal(Expression.PropertyOrField(param, propertyOrFieldName), Expression.Constant(value, value.GetType()));
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return data.Where(lambda.Compile());

            //var param = Expression.Parameter(typeof(Layout), "x");
            //var key = CurrentLayout.GetType().GetProperty(prop.Name);
            //var rhs = Expression.MakeMemberAccess(Expression.Constant(Convert.ChangeType(value, t)), key);
            //var lhs = Expression.MakeMemberAccess(param, key);
            //var body = Expression.Equal(lhs, rhs);
            //var lambda = Expression.Lambda<Func<Layout, bool>>(body, param);
        }
        public static IEnumerable<T> WhereFilter<T>(this ObservableCollection<T> data,
              string propertyOrFieldName, object value)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.Equal(Expression.PropertyOrField(param, propertyOrFieldName), Expression.Constant(value, value.GetType()));
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return data.Where(lambda.Compile());
        }


       public static bool IsConnectionEnabled()
        {
            bool result = false;
            if (Context != null)
            {
                var oldTimeOut = Context.XGetObjectContext().CommandTimeout;
                try
                {
                    Context.XGetObjectContext().CommandTimeout = 1;
                    Context.Database.Connection.Open();   // check the database connection
                    result = Context.Database.Connection.State == ConnectionState.Open;
                }
                catch
                {
                    result = false;
                }
                finally
                {
                    Context.XGetObjectContext().CommandTimeout = oldTimeOut;
                }
            }
            return result;
        }
    }


    //ObjectContext methods - low level for accsess to DataContext"
    public static partial class DataManager
    {
        public static string ConnectionString;
        public static DbContext DataToolsContext;

        public static class EntityHelper<T> where T : ObjectContext
        {
            public static T CreateInstance()
            {
                // get the connection string from config file
                string connectionString = ConfigurationManager.ConnectionStrings[typeof(T).Name].ConnectionString;

                // parse the connection string
                var csBuilder = new EntityConnectionStringBuilder(connectionString);

                // replace * by the full name of the containing assembly
                csBuilder.Metadata = csBuilder.Metadata.Replace(
                    "res://*/",
                    string.Format("res://{0}/", typeof(T).Assembly.FullName));

                // return the object
                return Activator.CreateInstance(typeof(T), csBuilder.ToString()) as T;
            }
        }

        public static string XGetEntitySetName<T>(this ObjectContext theContext, T eo) where T : EntityObject
        {
            var entitySetName = "";
            if (eo.EntityKey != null)
            {
                entitySetName = eo.EntityKey.EntitySetName;
            }
            else
            {
                var className = typeof(T).Name;
                var container = theContext.MetadataWorkspace.GetEntityContainer(theContext.DefaultContainerName, DataSpace.CSpace);
                entitySetName = (container.BaseEntitySets
                                          .Where(meta => meta.ElementType.Name == className)
                                          .Select(meta => meta.Name)).First();
            }
            return entitySetName;
        }

        public static void XGetEntities(this ObjectContext context)
        {
            //These two lines are needed to load the MetadataWorkspace.
            MetadataWorkspace mdw = ((EntityConnection)context.Connection).GetMetadataWorkspace();

            //Get the table info from Store Model
            var categoriesTable = mdw.GetItem<EntityType>("NorthwindModel.Store.Categories", DataSpace.SSpace);

            if (categoriesTable != null)
            {
                Console.WriteLine("Categories Table:");
                foreach (var prop in categoriesTable.Members)
                {
                    string info = String.Format("MemberName: {0} ; Type: {1}", prop.Name,
                                                prop.BuiltInTypeKind.ToString());
                    Console.WriteLine(info);
                }
                Console.ReadLine();

            }
        }

        public static IEnumerable<EntitySetBase> XGetEntitySets(this ObjectContext theContext)
        {
            var container = theContext.MetadataWorkspace.GetEntityContainer(theContext.DefaultContainerName, DataSpace.CSpace);
            return container.BaseEntitySets;
        }

        public static List<PropertyInfo> XGetEntityProperties(this ObjectContext context)
        {
            return context.GetType().GetProperties().ToList();
            //foreach (PropertyInfo prop in properties)
            //{
            //    if (prop.PropertyType.FullName != null && prop.PropertyType.FullName.Contains(typeof(ObjectSet<TEntity>).FullName))
            //    {
            //    }
            //}
        }

        public static IEnumerable<ObjectQuery> XGetObjectQueries(this ObjectContext theContext)
        {
            return theContext.GetType()
                             .GetProperties()
                             .Where(pd => pd.PropertyType.IsSubclassOf(typeof(ObjectQuery)))
                             .Select(pd => (ObjectQuery)pd.GetValue(theContext, null));
        }

        public static ObjectResult<DbDataRecord> XGetAllTypes(this ObjectContext context, string name)
        {
            ObjectResult<DbDataRecord> results = null;
            var metadataWorkspace = context.MetadataWorkspace;
            var entityContainers = metadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace);
            var namespaceNames = metadataWorkspace.GetItems<EntityType>(DataSpace.CSpace);
            if (entityContainers != null)
            {
                var container = entityContainers.First();
                if (namespaceNames != null)
                {
                    var namespaceName = namespaceNames.First().NamespaceName;
                    var setName = String.Empty;
                    var entityName = name + "Type";

                    var entitySetBase =
                        container.BaseEntitySets.FirstOrDefault(set => set.ElementType.Name == entityName);

                    if (entitySetBase != null) setName = entitySetBase.Name;

                    var entityType = metadataWorkspace.GetItem<EntityType>(namespaceName + "." + entityName,
                                                                           DataSpace.CSpace);

                    var stringBuilder = new StringBuilder().Append("SELECT entity ");
                    stringBuilder
                        .Append(" FROM " + container.Name.Trim() + "." + setName + " AS entity ");
                    var eSQL = stringBuilder.ToString();

                    ObjectQuery<DbDataRecord> query = context.CreateQuery<DbDataRecord>(eSQL);
                    results = query.Execute(MergeOption.AppendOnly);

                }
            }
            return results;
        }

        public static void XExecuteQuery(this ObjectContext context, string query)
        {
            context.ExecuteStoreQuery<int>(
                "SELECT COUNT(*) from information_schema.tables WHERE table_type = 'base table'");
        }
        public static void XExecuteQuery(this string connectionString, string query)
        {
            //var connection = ((EntityConnection)X2Context.Connection).StoreConnection as SqlConnection;
            //if (connection != null)
            var connection = new SqlConnection(connectionString);
            var cmd = new SqlCommand(query, connection); //"SELECT COUNT(*) from information_schema.tables WHERE table_type = 'base table'"
            connection.Open();
            var count = (int)cmd.ExecuteScalar();
            connection.Close();
        }

        public static int GetEntitySetCount(MetadataWorkspace workspace)
        {
            var count = 0;
            // Get a collection of the entity containers from storage space.
            var containers = workspace.GetItems<EntityContainer>(DataSpace.SSpace);

            foreach (var container in containers)
            {
                //Console.WriteLine("EntityContainer Name: {0} ", container.Name);
                foreach (var baseSet in container.BaseEntitySets)
                {
                    if (baseSet is EntitySet)
                    {
                        count++;
                        //Console.WriteLine("  EntitySet Name: {0} , EntityType Name: {1} ",
                        //    baseSet.Name, baseSet.ElementType.FullName);
                    }
                }
            }

            return count;
        }

        public class SqlConnector : IDisposable
        {
            //private readonly string _query;
            //private readonly string _connectionString;
            public readonly SqlConnection Connection;
            public SqlCommand Command;
            public int BatchSize = 100;

            public SqlConnector(string connectionString)
            {
                Connection = new SqlConnection(connectionString);
                Command = new SqlCommand("SELECT N'Testing Connection...'", Connection); //"SELECT COUNT(*) from information_schema.tables WHERE table_type = 'base table'"
                Connection.Open();
                //var count = (int)Cmd.ExecuteScalar();
            }

            public void Dispose()
            {
                Connection.Close();
            }
        }
    }


    /// <summary>  DatabaseBackUp, ... </summary>
    public static partial class DataManager
    {
        public static void DatabaseBackUp(Configuration config)
        {
            try
            {
                //var connectionString = db.Connection.ConnectionString;
                // read connectionstring from config file
                //var connectionString = ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;

                // read backup folder from config file ("C:/temp/")
                var backupFolder = "C:/temp/"; //config.AppSettings.Settings["BackupFolder"].Value;

                var connectionString = config.ConnectionStrings.ConnectionStrings["AcadNetContext"].ConnectionString;
                //var sqlConStrBuilder = new SqlConnectionStringBuilder(connectionString);

                // set backupfilename (you will get something like: "C:/temp/MyDatabase-2013-12-07.bak")
                var backupFileName = String.Format("{0}{1}-{2}.bak", backupFolder, "AcadNet",
                    //backupFolder, sqlConStrBuilder.InitialCatalog,
                    DateTime.Now.ToString("yyyy-MM-dd"));

                using (var connection = new SqlConnection(connectionString))
                {
                    var query = String.Format("BACKUP DATABASE {0} TO DISK='{1}'", "AcadNet", backupFileName);
                    //sqlConStrBuilder.InitialCatalog, backupFileName);

                    using (var command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                Console.WriteLine("KeyNotFoundException: " + ex.Message);
            }
            catch (System.FormatException ex)
            {
                Console.WriteLine("Format exception: " + ex.Message);
            }

        }
    }

//TEST
    public static partial class DataManager
    {
        private static IQueryable Exp()
        {
            var ids = new List<int>() { 1, 2, 3, 500 };
            using (var context = new TestContext())
            {
                //Expression<Func<Product, bool>> idMatching = null;
                //foreach (var id in ids)
                //{
                //    int productId = id;
                //    if (idMatching == null)
                //    {
                //        idMatching = x => x.CategoryId != null && x.CategoryId == Convert.ToString(productId);
                //    }
                //    else
                //    {
                //        //idMatching = idMatching.Or(x => x.CategoryId == productId);
                //    }
                //}
                //var products = context.Products.Where(idMatching);
                //WriteProducts(products);
            }
            return null;
        }

        public class TestContext : DbContext
        {
            //public DbSet<LO_Blocks> Blocks { get; set; }
            //public DbSet<Product> Products { get; set; }

            static TestContext()
            {
                Database.SetInitializer(new MyDbContextInitializer());
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                //modelBuilder.Entity<DataManager.Supplier>().Property(s => s.Name).IsRequired();
                Database.SetInitializer(new MyDbContextInitializer());
                base.OnModelCreating(modelBuilder);
            }
        }

        public class MyDbContextInitializer : DropCreateDatabaseAlways<TestContext> //DropCreateDatabaseIfModelChanges<XContext> //TEST
        {
            protected override void Seed(TestContext dbContext)
            {
                //var cat1 = new Category { CategoryId = "FOOD1", Name = ".NET Framework" };
                //var cat2 = new Category { CategoryId = "FOOD2", Name = "SQL Server" };
                //var cat3 = new Category { CategoryId = "FOOD3", Name = "jQuery" };

                //dbContext.Categories.Add(cat1);
                //dbContext.Categories.Add(cat2);
                //dbContext.Categories.Add(cat3);

                //dbContext.SaveChanges();
                base.Seed(dbContext);
            }
        }

        public class BlogXContextCustomInitializer : IDatabaseInitializer<TestContext> //TEST
        {
            public void InitializeDatabase(TestContext context)
            {
                if (context.Database.Exists())
                {
                    if (!context.Database.CompatibleWithModel(true))
                    {
                        context.Database.Delete();
                    }

                }
                context.Database.Create();
                context.Database.ExecuteSqlCommand("CREATE TABLE GLOBAL_DATA([KEY] VARCHAR(50), [VALUE] VARCHAR(250))");
            }
        }
    }
}