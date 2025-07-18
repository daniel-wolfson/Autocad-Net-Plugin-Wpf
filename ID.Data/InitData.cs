using Intellidesk.AcadNet.Data;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.DataContext;

namespace Intellidesk.Data
{
    public class InitData : RenameCreateDatabaseIfModelChanged<IntelliDesktopContext>
    {
        protected override void Seed(IntelliDesktopContext context)
        {
            base.Seed(context);
        }
    }
}