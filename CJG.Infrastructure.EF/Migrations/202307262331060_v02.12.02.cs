using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.12.02")]
    public partial class v021202 : ExtendedDbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DirectorBudgets", "WeeklyReceivablesNumber");
            DropColumn("dbo.DirectorBudgets", "WeeklyReceivablesTotal");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DirectorBudgets", "WeeklyReceivablesTotal", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.DirectorBudgets", "WeeklyReceivablesNumber", c => c.Int());
        }
    }
}
