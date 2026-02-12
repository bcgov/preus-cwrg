using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.03")]
    public partial class v021903 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "PreviousHourlyWage", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "PreviousHourlyWage");
        }
    }
}
