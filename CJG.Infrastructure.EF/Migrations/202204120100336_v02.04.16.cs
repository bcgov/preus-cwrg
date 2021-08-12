using System.ComponentModel;
namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.16")]
	public partial class v020416 : ExtendedDbMigration
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
