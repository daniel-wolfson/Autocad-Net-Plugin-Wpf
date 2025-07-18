using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Reflection;

namespace ID.Infrastructure.Filters
{
    public class ApiControllerCacheAttribute : ActionFilterAttribute
    {
        public ApiControllerCacheAttribute()
        {
            Init();
        }

        private void Init()
        {
            Assembly currentAssembly = typeof(ApiControllerCacheAttribute).Assembly;

            int noMethodsEncountered = 0;
            int noHits = 0;
            foreach (Type myType in currentAssembly.ExportedTypes.Where(x => x.Name.Contains("Controller")))
            {
                MethodInfo[] allMethods = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                ConstructorInfo[] allConstructors = myType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                foreach (MethodBase mi in allMethods.Concat((MethodBase[])allConstructors))
                {
                    noMethodsEncountered++;
                    if (mi.Name == "Preload")
                    {
                        foreach (ParameterInfo pi in mi.GetParameters())
                        {
                            if (pi.ParameterType.FullName != null && pi.ParameterType.FullName.Equals("System.Object"))
                            {
                                Console.WriteLine("Hit in type: {0} for method: {1}", myType.ToString(), mi.ToString());
                                noHits++;
                                break;      // break at the first occurrence as to not get multiple incorrect hits
                            }
                        }
                    }
                }
            }
        }
    }
}
