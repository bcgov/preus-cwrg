using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
    [Description("v02.04.08")]
	public partial class v020408 : ExtendedDbMigration
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
