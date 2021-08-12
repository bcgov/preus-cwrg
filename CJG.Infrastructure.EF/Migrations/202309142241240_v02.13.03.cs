using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.13.03")]
    public partial class v021303 : ExtendedDbMigration
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
