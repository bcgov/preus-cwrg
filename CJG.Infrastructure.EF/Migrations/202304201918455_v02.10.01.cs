using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.10.0")]
    public partial class v021001 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EligibleExpenseBreakdowns", "EnableCustomTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.ServiceLines", "EnableCustomTitle", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceLines", "EnableCustomTitle");
            DropColumn("dbo.EligibleExpenseBreakdowns", "EnableCustomTitle");
        }
    }
}
