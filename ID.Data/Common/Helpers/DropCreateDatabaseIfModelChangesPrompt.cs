using System.Data.Entity;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.DataContext;

namespace Intellidesk.Data.Common.Helpers
{
    public class DropCreateDatabaseIfModelChangesPrompt : DropCreateDatabaseIfModelChanges<IntelliDesktopContext>
    {
        public override void InitializeDatabase(IntelliDesktopContext context)
        {
            base.InitializeDatabase(context);
        }
    }
}