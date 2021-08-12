using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.16.02")]
    public partial class v021602 : ExtendedDbMigration
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
