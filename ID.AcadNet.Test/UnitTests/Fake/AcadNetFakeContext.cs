#region

using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories.EF6;

#endregion

namespace Intellidesk.AcadNet.Test.UnitTests.Fake
{
    public class AcadNetFakeContext : FakeDbContext
    {
        public AcadNetFakeContext()
        {
            AddFakeDbSet<Layout, LayoutDbSet>();
            AddFakeDbSet<UserSetting, UserSettingDbSet>();
            AddFakeDbSet<User, UserDbSet>();
            //AddFakeDbSet<Filter, FilterDbSet>();
            AddFakeDbSet<BlockDefinition, BlockDbSet>();
            AddFakeDbSet<BlockAttributeDefinition, BlockAttributeDbSet>();
            AddFakeDbSet<BlockRef, ItemDbSet>();
            AddFakeDbSet<BlockItemAttributeDef, ItemAttributeDbSet>();
            //AddFakeDbSet<Bay, BayDbSet>();
        }
    }
}
