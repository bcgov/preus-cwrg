using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.16.01")]
    public partial class v021601 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "TrainingExitDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "TrainingExitDate");
        }
    }
}
