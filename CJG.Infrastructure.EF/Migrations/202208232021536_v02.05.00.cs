using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
    [Description("v02.05.00")]
	public partial class v020500 : ExtendedDbMigration
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
