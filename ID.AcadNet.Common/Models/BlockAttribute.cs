using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.Data.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Models
{
    public partial class BlockAttribute
    {
        private string _name;
        private Dictionary<string, string> _atts;

        // Public read only properties
        public string Name
        {
            get { return _name; }
        }

        public Dictionary<string, string> Attributes => _atts;

        public string this[string key] => _atts[key.ToUpper()];

        // Constructors
        public BlockAttribute(BlockReference br)
        {
            SetProperties(br);
        }

        public BlockAttribute(ObjectId id)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                SetProperties(tr.GetObject(id, OpenMode.ForRead) as BlockReference);
            }
        }

        // Public method
        new public string ToString()
        {
            if (_atts != null && _atts.Count > 0)
                return $"{_name}: {_atts.Select(a => $"{a.Key}={a.Value}").Aggregate((a, b) => $"{a}; {b}")}";
            return _name;
        }

        // Private method
        private void SetProperties(BlockReference br)
        {
            //if (br == null) return;
            //_name = br.GetEffectiveName();
            //_atts = new Dictionary<string, string>();
            //br.AttributeCollection
            //    .GetObjects<AttributeReference>()
            //    .Iterate(att => _atts.Add(att.Tag.ToUpper(), att.TextString));
        }
    }

    [MetadataType(typeof(BlockAttributeMetaData))]
    public partial class BlockAttribute : BaseEntity
    {
    }
}