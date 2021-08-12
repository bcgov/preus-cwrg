using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.06")]
	public partial class v020406 : ExtendedDbMigration
    {
		public override void Up()
        {
            AddColumn("dbo.TrainingProviderTypes", "UseForProviderTypes", c => c.Int(nullable: false));

			PostDeployment();
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingProviderTypes", "UseForProviderTypes");
        }
    }
}
