using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.18.01")]
	public partial class v021801 : ExtendedDbMigration
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
