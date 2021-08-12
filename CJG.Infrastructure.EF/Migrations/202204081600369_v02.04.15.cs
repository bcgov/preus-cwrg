using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.15")]
	public partial class v020415 : ExtendedDbMigration
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
