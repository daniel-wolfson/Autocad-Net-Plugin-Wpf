using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;
using Intellidesk.Data.Repositories;

namespace Intellidesk.Data.Models.Cad
{
    [MetadataType(typeof(UserMetaData))]
    public partial class User : BaseEntity
    {
        public User()
        {
            this.UserSettings = new List<UserSetting>();
        }

        public User(int userId, string password, int role)
        {
            UserId = userId;
            Password = password;
            Role = role;
        }

        public int UserId { get; set; }
        public int UserSettingId { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Settings_Data { get; set; }
        public string Drive { get; set; }
        public bool IsBlocked { get; set; }
        public int Role { get; set; } // 1 = manager
        public string Password { get; set; }
        public string Token { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
    }

    public partial class User
    {
        private UserSetting[] _settings = null;
        [NotMapped]
        public UserSetting[] Settings
        {
            get
            {
                if (_settings == null)
                {
                    var _items = new List<UserSetting>();
                    var innerList = Settings_Data.XParse<UserSetting>();
                    _items.AddRange(innerList);
                    return _items.ToArray();
                }
                return _settings;
            }
            set
            {
                var xml = new StringBuilder();
                foreach (var setting in value.ToList())
                {
                    var xmlOver = new XmlAttributeOverrides();
                    var xmlAttr = new XmlAttributes {XmlIgnore = true};

                    xmlOver.Add(typeof(UserSetting), "Users", xmlAttr);
                    xmlOver.Add(typeof(UserSetting), "Layout", xmlAttr);

                    var xs = new XmlSerializer(typeof(UserSetting), xmlOver);
                    var sw = new StringWriter();
                    var settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
                    var writer = XmlWriter.Create(sw, settings);
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");

                    xs.Serialize(writer, setting, ns);
                    xml.Append(sw);
                }
                Settings_Data = xml.ToString();
                _settings = value;
            }
        }

        public User EntityByDefault()
        {
            var User = new User()
            {
                Drive = "C:",
                Name = Environment.UserName,
                UserId = 0,
                UserSettingId = 0,
                Email = Environment.UserName + "@gmail.com", //2013-04-28T15:19:36.4384559+03:00
                Settings_Data = String.Format("<UserSettings><UserSetting><UserSettingId>0</UserSettingId><ConfigSetName>Default</ConfigSetName>" +
                            "<ChainDistance>2</ChainDistance><DateStarted>{0}</DateStarted><Drive>C:</Drive><Id>42</Id>" +
                            "<IsActive>true</IsActive><IsColorMode>true</IsColorMode>" +
                            "<LayoutId>0</LayoutId>" +
                            "<ProjectExplorerRowSplitterPosition>140</ProjectExplorerRowSplitterPosition>" +
                            "<ProjectExplorerPGridColumnSplitterPosition>40</ProjectExplorerPGridColumnSplitterPosition><Percent>0</Percent>" +
                            "<ProjectStatus>New</ProjectStatus><ToggleLayoutDataTemplateSelector>0</ToggleLayoutDataTemplateSelector>" +
                            "<MinWidth>376</MinWidth></UserSetting></UserSettings>", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc))
            };
            return User;
        }
    }
}
