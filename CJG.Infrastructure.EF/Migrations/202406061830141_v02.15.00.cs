using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.15.00")]
    public partial class v021500 : ExtendedDbMigration
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
