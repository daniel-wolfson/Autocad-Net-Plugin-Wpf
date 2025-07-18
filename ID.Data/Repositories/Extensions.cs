using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Intellidesk.Data.Repositories
{
    // Exmaple: How to add custom methods to a repository.
    public static class Extensions
    {
        public static IEnumerable<TEntity> WhenEmpty<TEntity>(this IEnumerable<TEntity> listItems, Func<TEntity> onEmpty) where TEntity : BaseEntity, new()
        {
            //IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            //if (pluginSettings.IsDemo || listItems == null || !listItems.Any())
            return listItems ?? new List<TEntity> { onEmpty() };
        }

        public static ObservableCollection<TEntity> ToItems<TEntity>(this IEnumerable<TEntity> listItems,
            EventHandler<EntityChangedArgs> onItemChanged = null) where TEntity : BaseEntity, new()
        {
            var observableItems = new ObservableCollection<TEntity>();
            var items = listItems.ToList();
            items.ForEach(x =>
            {
                if (onItemChanged != null)
                    x.EntityChangedEvent += onItemChanged;
                observableItems.Add(x);
            });
            return observableItems;
        }

        public static void Clear<TEntity>(this IEnumerable<TEntity> listItems,
            EventHandler<EntityChangedArgs> onItemChanged = null) where TEntity : BaseEntity
        {
            var items = listItems.ToList();
            items.ForEach(x =>
            {
                if (onItemChanged != null)
                    x.EntityChangedEvent -= onItemChanged;
            });
        }

        public static ObservableCollection<TEntity> WhenItemChanged<TEntity>(this ObservableCollection<TEntity> items,
            EventHandler<EntityChangedArgs> triggerOnEntityChange) where TEntity : BaseEntity
        {
            items.ForEach(x => x.EntityChangedEvent += triggerOnEntityChange);
            return items;
        }

        public static ObservableCollection<TEntity> ToObservableCollection<TEntity>(this IRepository<TEntity> repository,
            EventHandler<EntityChangedArgs> triggerOnEntityChange = null) where TEntity : BaseEntity
        {
            var observableItems = new ObservableCollection<TEntity>();
            repository.Queryable().ForEach(x =>
            {
                if (triggerOnEntityChange != null)
                    x.EntityChangedEvent += triggerOnEntityChange;
                observableItems.Add(x);
            });

            return observableItems;
        }

        public static IEnumerable<TEntity> XParse<TEntity>(this string xmlContext) where TEntity : BaseEntity
        {
            var doc = new XmlDocument();
            var result = new List<TEntity>();

            if (xmlContext != "")
                doc.LoadXml(xmlContext);
            else
                return new List<TEntity>();

            try
            {
                var root = doc.DocumentElement;
                if (root != null)
                {
                    var elemList = root.GetElementsByTagName(typeof(TEntity).Name);

                    foreach (XmlNode item in elemList)
                    {
                        var xs = new XmlSerializer(typeof(TEntity));
                        var stream = new MemoryStream(Encoding.UTF8.GetBytes(item.OuterXml));

                        var sr = new StreamReader(stream);
                        var itemResult = (TEntity)xs.Deserialize(sr);
                        result.Add(itemResult);
                        sr.Close();
                    }
                }
            }
            catch (Exception)
            {
                // File not found: create default settings
                return (new List<TEntity>()).AsEnumerable(); //{ FilterName = "Default", Active = false };
            }
            return result.AsEnumerable();
        }
    }
}