using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.01")]
    public partial class v020401 : ExtendedDbMigration
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
