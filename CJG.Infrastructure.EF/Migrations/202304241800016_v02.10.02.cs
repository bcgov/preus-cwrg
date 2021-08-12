using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.10.02")]
	public partial class v021002 : ExtendedDbMigration
    {
		public override void Up()
        {
            AddColumn("dbo.EligibleCostBreakdowns", "CustomCostTitle", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EligibleCostBreakdowns", "CustomCostTitle");
        }
    }
}
