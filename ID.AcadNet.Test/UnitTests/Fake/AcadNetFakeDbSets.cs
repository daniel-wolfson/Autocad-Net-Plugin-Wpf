using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.AcadNet.Test.UnitTests.Fake
{
    public class LayoutDbSet : FakeDbSet<Layout>
    {
        public override Layout Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.LayoutID == (int)keyValues.FirstOrDefault());
        }

        public override Task<Layout> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<Layout>(() => Find(keyValues));
        }
    }

    public class UserDbSet : FakeDbSet<User>
    {
        public override User Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.UserId.ToString() == (string)keyValues.FirstOrDefault());
        }

        public override Task<User> FindAsync(params object[] keyValues)
        {
            return new Task<User>(() => Find(keyValues));
        }

        public override Task<User> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<User>(() => Find(keyValues));
        }
    }
    public class UserSettingDbSet : FakeDbSet<UserSetting>
    {
        public override UserSetting Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.UserSettingId == (int)keyValues.FirstOrDefault());
        }

        public override Task<UserSetting> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<UserSetting>(() => this.SingleOrDefault(t => t.UserSettingId == (int)keyValues.FirstOrDefault()));
        }
    }

    public class BlockDbSet : FakeDbSet<BlockDefinition>
    {
        public override BlockDefinition Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.LayoutId == (int)keyValues[0] && t.BlockId == (int)keyValues[1]);
        }

        public override Task<BlockDefinition> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<BlockDefinition>(() => this.SingleOrDefault(t => t.LayoutId == (int)keyValues[0] && t.BlockId == (int)keyValues[1]));
        }
    }
    public class BlockAttributeDbSet : FakeDbSet<BlockAttributeDefinition>
    {
        public override BlockAttributeDefinition Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.BlockAttributeId == (int)keyValues.FirstOrDefault());
        }

        public override Task<BlockAttributeDefinition> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<BlockAttributeDefinition>(() => this.SingleOrDefault(t => t.BlockAttributeId == (int)keyValues.FirstOrDefault()));
        }
    }
    
    public class ItemDbSet : FakeDbSet<BlockRef>
    {
        public override BlockRef Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.LayoutId == (int)keyValues[0] && t.BlockRefId == (int)keyValues[1]);
        }

        public override Task<BlockRef> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<BlockRef>(() => this.SingleOrDefault(t => t.LayoutId == (int)keyValues[0] && t.BlockRefId == (int)keyValues[1]));
        }
    }
    public class ItemAttributeDbSet : FakeDbSet<BlockItemAttributeDef>
    {
        public override BlockItemAttributeDef Find(params object[] keyValues)
        {
            return this.SingleOrDefault(t => t.BlockItemAttributeDefId == (int)keyValues.FirstOrDefault());
        }

        public override Task<BlockItemAttributeDef> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return new Task<BlockItemAttributeDef>(() => this.SingleOrDefault(t => t.BlockItemAttributeDefId == (int)keyValues.FirstOrDefault()));
        }
    }

    //public class RuleDbSet : FakeDbSet<Rule>
    //{
    //    public override Rule Find(params object[] keyValues)
    //    {
    //        return this.SingleOrDefault(t => t.RuleId == (int)keyValues.FirstOrDefault());
    //    }

    //    public override Task<Rule> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
    //    {
    //        return new Task<Rule>(() => this.SingleOrDefault(t => t.RuleId == (int)keyValues.FirstOrDefault()));
    //    }
    //}
}