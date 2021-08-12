using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.16.04")]
    public partial class v021604 : ExtendedDbMigration
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
