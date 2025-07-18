using ID.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Intellidesk.Data.General
{
    [DataContract]
    public class WorkItem : IWorkItem
    {
        public WorkItem()
        {
        }

        public WorkItem(string fullPath)
        {
            FullPath = fullPath;
            WorkTypeName = "CommandArgs";
        }

        public WorkItem(string fullPath, string workTypeName) : this(fullPath)
        {
            FullPath = fullPath;
            WorkTypeName = string.IsNullOrEmpty(workTypeName) ? "CommandArgs" : workTypeName;
        }

        public WorkItem(string fullPath, string workTypeName, string work) : this(fullPath, workTypeName)
        {
            Work = work;
        }

        [JsonProperty("FullPath")]
        public string FullPath { get; set; }
        [JsonProperty("Work")]
        public string Work { get; set; }
        [JsonProperty("WorkTypeName")]
        public string WorkTypeName { get; set; }
    }
}