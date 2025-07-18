using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ID.Infrastructure
{
    public class AppAssemblyResolver
    {
        public static Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = args.Name.Contains(',')
                ? args.Name.Substring(0, args.Name.IndexOf(','))
                : args.Name;

            string[] LoadAssemblies = {
                "Newtonsoft.Json",
                "System.Runtime.CompilerServices.Unsafe",
                "Microsoft.Extensions.Configuration.Abstractions",
                "Microsoft.Extensions.Configuration.Binder",
                "Microsoft.Extensions.Logging.Abstractions",
                "System.Runtime.CompilerServices.Unsafe",
                "Microsoft.Owin.Security",
                "System.Threading.Tasks.Extensions" }; // Forbid non handled dll's

            if (!LoadAssemblies.Contains(assemblyFile)) return null;

            string absoluteFolder = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Directory.FullName;
            string targetPath = Path.Combine(absoluteFolder, "Dlls", assemblyFile + ".dll");

            try
            {
                return Assembly.LoadFile(targetPath);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
