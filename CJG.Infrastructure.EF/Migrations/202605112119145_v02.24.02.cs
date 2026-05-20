using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.24.02")]
    public partial class v022402 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "EiEligibilityReportedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "EiEligibilityReportedOn");
        }
    }
}
