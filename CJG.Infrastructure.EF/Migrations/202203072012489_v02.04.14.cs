using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.11")]
	public partial class v020414 : ExtendedDbMigration
	{
        public override void Up()
        {
            AddColumn("dbo.TrainingPrograms", "SkillsTrainingFocusType", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingPrograms", "SkillsTrainingFocusType");
        }
    }
}
