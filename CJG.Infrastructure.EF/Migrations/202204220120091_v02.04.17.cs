using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.17")]
	public partial class v020417 : ExtendedDbMigration
	{
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "SinReportedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "SinReportedOn");
        }
    }
}
