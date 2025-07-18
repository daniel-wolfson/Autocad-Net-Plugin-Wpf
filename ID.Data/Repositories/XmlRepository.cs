using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Repositories.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Intellidesk.Data.Repositories
{
    public class XmlRepository<TEntity> : Repository<TEntity>, IXmlRepository<TEntity> where TEntity : class, IObjectState, new()
    {
        public static IPluginSettings PluginSettings
        {
            get { return Plugin.GetService<IPluginSettings>(); }
        }

        public string XmlFileName { get; set; }

        public XmlRepository(string xmlFileName, IUnitOfWorkAsync unitOfWork)
            : base(xmlFileName, unitOfWork)
        {
            XmlFileName = xmlFileName;
        }

        public IEnumerable<TEntity> Load(string xmlFileName = "")
        {
            var result = new List<TEntity>();

            if (xmlFileName != "")
                XmlFileName = xmlFileName;

            try
            {
                var doc = new XmlDocument();
                doc.Load(PluginSettings.RootPath + XmlFileName + ".xml");

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

        public TEntity LoadSingleObject(string filename = "")
        {
            TEntity result;

            if (filename != "")
                XmlFileName = filename;

            try
            {
                var xs = new XmlSerializer(typeof(TEntity));
                var sr = new StreamReader(PluginSettings.RootPath + XmlFileName + ".xml");
                result = (TEntity)xs.Deserialize(sr);
                sr.Close();
            }
            catch (Exception)
            {
                // File not found: create default settings
                return new TEntity(); //{ FilterName = "Default", Active = false };
            }
            return result;
        }

        public void Save()
        {
            try
            {
                var xs = new XmlSerializer(typeof(IEnumerable<TEntity>));
                var sw = new StreamWriter(PluginSettings.RootPath + XmlFileName, false);
                xs.Serialize(sw, this);
                sw.Close();
            }
            catch (Exception)
            {
                //var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                //ed.WriteMessage("\nUnable to save the application filters: {0}", ex);
            }
        }
    }
}