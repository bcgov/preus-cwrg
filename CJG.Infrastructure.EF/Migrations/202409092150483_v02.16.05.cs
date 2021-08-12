using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.16.05")]
	public partial class v021605 : ExtendedDbMigration
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
