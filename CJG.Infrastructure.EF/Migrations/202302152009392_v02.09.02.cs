using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.09.02")]
	public partial class v020902 : ExtendedDbMigration
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
