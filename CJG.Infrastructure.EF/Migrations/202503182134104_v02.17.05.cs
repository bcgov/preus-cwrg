using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.17.05")]
    public partial class v021705 : ExtendedDbMigration
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
