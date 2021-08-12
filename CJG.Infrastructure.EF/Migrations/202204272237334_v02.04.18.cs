using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.18")]
    public partial class v020418 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "DuplicateSinReportedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "DuplicateSinReportedOn");
        }
    }
}
