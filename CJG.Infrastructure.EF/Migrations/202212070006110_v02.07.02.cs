using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.07.02")]
    public partial class v020702 : ExtendedDbMigration
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
