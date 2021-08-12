using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.05")]
	public partial class v020405 : ExtendedDbMigration
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
