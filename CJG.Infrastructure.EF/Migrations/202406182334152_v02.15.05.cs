using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.15.05")]
    public partial class v021505 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "ExitSurveyReportedOn", c => c.DateTime());
            AddColumn("dbo.ParticipantForms", "EarlyWithdrawalReportedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "EarlyWithdrawalReportedOn");
            DropColumn("dbo.ParticipantForms", "ExitSurveyReportedOn");
        }
    }
}
