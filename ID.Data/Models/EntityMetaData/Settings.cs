using System;
using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.EntityMetaData
{
    public class Settings : BaseEntity
    {
        public string UserName { get; set; }
        public string Data { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string Setting { get; set; }
    }
}