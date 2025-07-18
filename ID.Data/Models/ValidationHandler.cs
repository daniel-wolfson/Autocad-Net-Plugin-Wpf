using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Intellidesk.Data.Models
{
    public class ValidationHandler
    {
        //BrokenRules
        private Dictionary<string, string> InvalidProperties { get; set; }
        private Dictionary<string, object> ChangedProperties { get; set; }
        //protected Lazy<Dictionary<string, string>> InvalidProperties = new Lazy<Dictionary<string, string>>();
        
        public ValidationHandler()
        {
            InvalidProperties = new Dictionary<string, string>();
            ChangedProperties = new Dictionary<string, object>();
        }
        public string this[string property]
        {
            get { return InvalidProperties[property]; }
        }
        public bool InvalidPropertyExist(string property)
        {
            return InvalidProperties.ContainsKey(property);
        }
        public bool InvalidPropertiesExist()
        {
            return InvalidProperties.Any();
        }

        public Dictionary<string, string> GetInvalidProperties()
        {
            return InvalidProperties;
        }
        public string ValidateRule(string property, string message, Func<bool> ruleCheck)
        {
            var result = "";
            bool check = ruleCheck();
            if (!check)
            {
                if (InvalidPropertyExist(property))
                    RemoveInvalidProperty(property);

                InvalidProperties.Add(property, message);
                result = message;
            }
            else
            {
                RemoveInvalidProperty(property);
            }
            return result;  //check;
        }

        public void RemoveInvalidProperty(string property)
        {
            if (InvalidProperties.ContainsKey(property))
            {
                InvalidProperties.Remove(property);
            }
        }
        
        public void Clear()
        {
            InvalidProperties.Clear();
        }

        public string FindFileRuleAsync(string fileName, string ext, out string driveResult)
        {
            //var driveResult = "";
            var messageResult = "File not found";
            driveResult = "";
            var result = "";

            ManualResetEventSlim manualResetEvent = new ManualResetEventSlim();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;


            if (!fileName.Contains(ext)) return messageResult;

            string[] driveNames = Directory.GetLogicalDrives();
            var tasks = new List<Task<string>>();
            foreach (var driveName in driveNames)
            {
                string drive = driveName;
                Debug.Write("\n" + drive + fileName);
                var t = Task.Run(() =>
                {
                    //token.ThrowIfCancellationRequested();

                    if (!token.IsCancellationRequested)
                    {
                        if (File.Exists(drive + fileName))
                        {
                            //manualResetEvent.Set();
                            messageResult = "";
                            return drive.Substring(0, drive.IndexOf(":") + 1);
                        }
                        //else token.ThrowIfCancellationRequested();
                        Debug.Write(" ... " + messageResult);
                    }
                    else
                    {
                        Debug.Write(" ... canceled");
                        //throw new OperationCanceledException(token);
                    }
                    return "";
                }, token);
                tasks.Add(t);
            }

            try
            {
                while (tasks.Any())
                {
                    int taskIndex = Task.WaitAny(tasks.ToArray());
                    if (tasks[taskIndex].Status == TaskStatus.RanToCompletion && tasks[taskIndex].Result != "")
                    {
                        result = tasks[taskIndex].Result;
                        tokenSource.Cancel();
                    }
                    tasks.Remove(tasks[taskIndex]);
                }
            }
            catch (AggregateException e)
            {
                foreach (var v in e.InnerExceptions)
                    messageResult += e.Message + " " + v.Message;
            }

            driveResult = result;
            return messageResult;
        }
    }
}