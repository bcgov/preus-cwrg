using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.02")]
	public partial class v020402 : ExtendedDbMigration
	{
        public override void Up()
        {
            DropForeignKey("dbo.TrainingPrograms", "SkillLevelId", "dbo.SkillLevels");
            DropIndex("dbo.TrainingPrograms", "IX_TrainingPrograms");
            AlterColumn("dbo.TrainingPrograms", "SkillLevelId", c => c.Int());
            CreateIndex("dbo.TrainingPrograms", new[] { "TrainingProgramState", "DateAdded", "GrantApplicationId", "StartDate", "EndDate", "InDemandOccupationId", "SkillLevelId", "SkillFocusId", "ExpectedQualificationId", "TrainingLevelId" }, name: "IX_TrainingPrograms");
            AddForeignKey("dbo.TrainingPrograms", "SkillLevelId", "dbo.SkillLevels", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingPrograms", "SkillLevelId", "dbo.SkillLevels");
            DropIndex("dbo.TrainingPrograms", "IX_TrainingPrograms");
            AlterColumn("dbo.TrainingPrograms", "SkillLevelId", c => c.Int(nullable: false));
            CreateIndex("dbo.TrainingPrograms", new[] { "TrainingProgramState", "DateAdded", "GrantApplicationId", "StartDate", "EndDate", "InDemandOccupationId", "SkillLevelId", "SkillFocusId", "ExpectedQualificationId", "TrainingLevelId" }, name: "IX_TrainingPrograms");
            AddForeignKey("dbo.TrainingPrograms", "SkillLevelId", "dbo.SkillLevels", "Id", cascadeDelete: true);
        }
    }
}
