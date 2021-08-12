using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.07.03")]
    public partial class v020703 : ExtendedDbMigration
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
