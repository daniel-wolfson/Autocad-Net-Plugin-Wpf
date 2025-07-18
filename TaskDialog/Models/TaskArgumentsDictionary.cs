using System.Collections.Generic;

namespace Intellidesk.Infrastructure.Tasks
{
    public class TaskArgumentsDictionary
    {
        private readonly IDictionary<string, string> _yourDictionary = new Dictionary<string, string>();

        public string this[string key]
        {
            // returns value if exists
            get { return _yourDictionary[key]; }

            // updates if exists, adds if doesn't exist
            set { _yourDictionary[key] = value; }
        }
    }
}