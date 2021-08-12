using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.07.01")]
    public partial class v020701 : ExtendedDbMigration
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
