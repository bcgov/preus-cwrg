using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.24.01")]
	public partial class v022401 : ExtendedDbMigration
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
