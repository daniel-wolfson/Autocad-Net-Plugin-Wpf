using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;

namespace Intellidesk.Data.Repositories
{
    /// <summary> Simulates a Layout data source, which would normally come from a database </summary>
    //[Export(typeof(ILayoutRepository))]
    public class LayoutRepository : Repository<Models.Cad.ILayout>
    {
        private static readonly string _filename = @"Models\LayoutSettings.xml";

        public LayoutRepository(IDataContextAsync context, IUnitOfWorkAsync uow)
            : base(context, uow)
        {
        }

        // Get Layouts Settings
        public IEnumerable<Models.Cad.ILayout> GetLayoutsSettings()
        {
            if (!File.Exists(_filename))
            {
                throw new FileNotFoundException("DataSource file could not be found", _filename);
            }

            var layouts = System.Xml.Linq.XDocument.Load(_filename).Root.Elements("Layout");
                //.Select(x => (Models.Cad.ILayout)new Layout()
                //{
                //    LayoutName = x.Element("Title").ToString(),
                //    CreatedBy = 0, //X.Element("CreatedBy").ToString(),
                //    ModifiedBy = 0, //X.Element("ModifiedBy").ToString()
                //});
            return null;
        }

        public new void UpdateRange(IEnumerable<Models.Cad.ILayout> layouts)
        {
            if (layouts == null)
                throw new ArgumentNullException("layouts");

            new System.Xml.Linq.XDocument(layouts).Save(_filename);
        }
    }

    //[Export(typeof(IUserStatesRepository))]
    //public class StateRepository : IUserStatesRepository
    //{
    //    private static readonly string _filename = @"Models\States.xml";

    //    // Get Layouts Settings
    //    public IEnumerable<State> GetStates()
    //    {
    //        if (!File.Exists(_filename))
    //        {
    //            throw new FileNotFoundException("DataSource file could not be found", _filename);
    //        }

    //        var items = System.Xml.Linq.XDocument.Load(_filename).Root.Elements("Item").Select(
    //            X => new State() {
    //                AsMade = (string)X.Element("AsMade"), 
    //                Lon = (float)X.Element("Lon"), 
    //                Lat = (float)X.Element("Lat"),
    //                DateCreated = Convert.ToDateTime(X.Element("DateCreated"))
    //            });
    //        return items;
    //    }

    //    // Saves layouts
    //    public void SetStates(IEnumerable<State> states)
    //    {
    //        if (states == null)
    //            throw new ArgumentNullException("states");

    //        new System.Xml.Linq.XDocument(states).Save(_filename);
    //    }

    //    public void SetUpdate<T>(T entity, string propertyName, object value) where T : class
    //    {
    //        using (var context = DataManager.CreateContext<MapinfoContext>())
    //        {
    //            if (context != null)
    //            {
    //                try
    //                {
    //                    context.Configuration.ValidateOnSaveEnabled = false;
    //                    if (context.Entry(entity).State == EntityState.Detached)
    //                        context.Set<T>().Attach(entity);

    //                    entity.GetType().GetProperty(propertyName).SetValue(entity, value, null);
    //                    context.Entry(entity).Property(propertyName).IsModified = true;

    //                    //context.Entry(entity).State = EntityState.Modified;
    //                    context.SaveChanges();
    //                }
    //                catch (Exception)
    //                {

    //                }
    //            }
    //        }
    //    }

    //    public void SetUpdate<T>(T entity, params string[] properties) where T : class
    //    {
    //        using (var context = DataManager.CreateContext<MapinfoContext>())
    //        {
    //            if (context != null)
    //            {
    //                try
    //                {
    //                    context.Configuration.ValidateOnSaveEnabled = false;
    //                    if (context.Entry(entity).State == EntityState.Detached)
    //                        context.Set<T>().Attach(entity);

    //                    if (properties.Length > 0)
    //                        foreach (var property in properties)
    //                            context.Entry(entity).Property(property).IsModified = true;

    //                    //context.Entry(entity).State = EntityState.Modified;
    //                    context.SaveChanges();
    //                }
    //                catch (Exception)
    //                {

    //                }
    //            }
    //        }
    //    }

    //    // Get Layouts
    //    public IEnumerable<State> LoadStates()
    //    {
    //        var states = new List<State>();
    //        using (var context = DataManager.CreateContext<MapinfoContext>())
    //        {
    //            if (context != null)
    //            {
    //                try
    //                {
    //                    context.State.ToList().ForEach(X =>
    //                        states.Add(new State
    //                        {
    //                            AsMade = X.AsMade,
    //                            Lat = X.Lat,
    //                            Lon = X.Lon,
    //                            DateCreated = X.DateCreated
    //                        }));
    //                }
    //                catch (Exception)
    //                {

    //                }
    //            }
    //        }
    //        return states;
    //    }
    //}

}