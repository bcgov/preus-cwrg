using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.08.02")]
	public partial class v020802 : ExtendedDbMigration
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
