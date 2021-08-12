using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.12")]
    public partial class v020412 : ExtendedDbMigration
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
