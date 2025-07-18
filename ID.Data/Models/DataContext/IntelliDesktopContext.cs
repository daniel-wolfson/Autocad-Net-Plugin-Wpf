using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Mapping;
using Intellidesk.Data.Services;
using System.Data.Entity;

namespace Intellidesk.Data.Models.DataContext
{
    public class IntelliDesktopContext : Repositories.EF6.DataContext.DataContext
    {
        static IntelliDesktopContext()
        {
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<IntelliDesktopContext>());
            //Database.SetInitializer(new CreateDatabaseIfNotExists<IntelliDesktopContext>());
            //Database.SetInitializer(new DropCreateDatabaseTables());
            //Database.SetInitializer(new IntelliDesktopDbInitializer());
            //Database.SetInitializer(new InitData());
        }

        public IntelliDesktopContext()
            : base(ConfigurationManager<IntelliDesktopContext>.ConnectionString)
        {
            IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            pluginSettings.IsDemo = !IsValid; //GIGABYTE-PC

            //var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            //objectContext.ObjectMaterialized += (sender, e) =>
            //{
            //    //var user = e.Entity as User;
            //    //if (e != null)
            //    //{
            //    //    // Now you can call your initialization logic
            //    //    //user.Settings = settings;
            //    //}
            //};
        }

        public DalCollection<BlockDefinition> Blocks { get; set; }
        public DalCollection<BlockRef> BlockItems { get; set; }
        public DalCollection<BlockAttributeDefinition> BlockAttributes { get; set; }
        public DalCollection<BlockItemAttributeDef> BlockItemAttributes { get; set; }
        public DalCollection<ILayout> Layouts { get; set; }
        public DalCollection<User> Users { get; set; }
        public DalCollection<UserSetting> UserSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new LayoutMap());
            modelBuilder.Configurations.Add(new ClosureMap());

            modelBuilder.Configurations.Add(new BlockMap());
            modelBuilder.Configurations.Add(new BlockAttributeMap());

            modelBuilder.Configurations.Add(new BlockItemMap());
            modelBuilder.Configurations.Add(new BlockItemAttributeDefinitionMap());

            modelBuilder.Configurations.Add(new LayoutMap());
            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new UserSettingMap());
        }
    }
}
