using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.20.03")]
    public partial class v022003 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "LMDAEligible", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "LMDAEligible");
        }
    }
}
