using System.IO;
using System.Reflection;
using System.Xml;

namespace ID.Api.Models
{
    public class EmailTemplateBuilder
    {
        public string Link { get; set; }

        public EmailTemplateBuilder(string link)
        {
            Link = link;
        }

        public XmlDocument BuildTemplate(string templateName = "AmanEmailTemplate")
        {
            var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{templateName}.xml");
            //Assembly a = typeof(Startup).Assembly;
            //Stream s = a.GetManifestResourceStream($"Psygate.WebApi.{fileName}.xml");
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(fileName);
            //s.Close();
            return xdoc;
        }
    }
}

