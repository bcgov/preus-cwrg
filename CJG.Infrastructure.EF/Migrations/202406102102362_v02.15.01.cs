using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.15.01")]
    public partial class v021501 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GrantStreams", "IsCoreStream", c => c.Boolean(nullable: false));

			PostDeployment();
        }
        
        public override void Down()
        {
            DropColumn("dbo.GrantStreams", "IsCoreStream");
        }
    }
}
