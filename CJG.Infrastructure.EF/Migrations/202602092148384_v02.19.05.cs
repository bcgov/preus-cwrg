using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.05")]
    public partial class v021905 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "PreviousEmployerFullName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "PreviousEmployerFullName");
        }
    }
}
