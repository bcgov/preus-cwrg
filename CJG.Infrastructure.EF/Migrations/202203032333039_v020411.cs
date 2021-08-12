using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.11")]
    public partial class v020411 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "AvgHoursPerWeekDuringTraining", c => c.Int());

			PostDeployment();
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "AvgHoursPerWeekDuringTraining");
        }
    }
}
