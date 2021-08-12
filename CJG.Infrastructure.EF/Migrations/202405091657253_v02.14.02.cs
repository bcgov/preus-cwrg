using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.14.02")]
	public partial class v021402 : ExtendedDbMigration
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
