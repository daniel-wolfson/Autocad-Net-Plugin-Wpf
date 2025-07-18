using System;

namespace ID.Infrastructure.Models
{
    [Serializable]
    public class ApiReportItem
    {
        public ApiReportItem(string name, object data)
        {
            Name = name; Data = data;
        }
        //public string Key { get; set; }
        public string Name { get; set; }
        public object Data { get; set; }
        public DateTime Started => DateTime.Now;
    }
}
