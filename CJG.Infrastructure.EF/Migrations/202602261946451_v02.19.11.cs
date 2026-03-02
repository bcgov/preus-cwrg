using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.11")]
	public partial class v021911 : ExtendedDbMigration
    {
        public override void Up()
        {
			PostDeployment();
        }
        
        public override void Down()
        {
        }
    }
}
