using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.04")]
    public partial class v021904 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "PreviousAvgHoursPerWeek", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "PreviousAvgHoursPerWeek");
        }
    }
}
