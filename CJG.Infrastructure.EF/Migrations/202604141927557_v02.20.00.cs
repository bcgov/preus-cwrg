using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.20.00")]
    public partial class v022000 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingCosts", "CommittedLMDA", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.TrainingCosts", "CommittedWDA", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingCosts", "CommittedWDA");
            DropColumn("dbo.TrainingCosts", "CommittedLMDA");
        }
    }
}
