using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.02")]
    public partial class v021902 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "PreviousEmploymentLastDayOfWork", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "PreviousEmploymentLastDayOfWork");
        }
    }
}
