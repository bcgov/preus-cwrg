using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.15.04")]
	public partial class v021504 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.ParticipantExitSurveyAnswers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParticipantExitSurveyQuestionOptionId = c.Int(nullable: false),
                        ParticipantFormId = c.Int(nullable: false),
                        OptionTextDisplayed = c.String(),
                        Answer = c.Boolean(nullable: false),
                        TextAnswer = c.String(),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantExitSurveyQuestionOptions", t => t.ParticipantExitSurveyQuestionOptionId, cascadeDelete: true)
                .ForeignKey("dbo.ParticipantForms", t => t.ParticipantFormId, cascadeDelete: true)
                .Index(t => t.ParticipantExitSurveyQuestionOptionId)
                .Index(t => t.ParticipantFormId);
            
            CreateTable(
                "dbo.ParticipantExitSurveyQuestionOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParticipantExitSurveyQuestionId = c.Int(nullable: false),
                        OptionText = c.String(),
                        ReplacementToken = c.String(),
                        AllowOther = c.Boolean(nullable: false),
                        Sequence = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantExitSurveyQuestions", t => t.ParticipantExitSurveyQuestionId, cascadeDelete: true)
                .Index(t => t.ParticipantExitSurveyQuestionId);
            
            CreateTable(
                "dbo.ParticipantExitSurveyQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionType = c.Int(nullable: false),
                        Question = c.String(),
                        Sequence = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);

            PostDeployment();
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantExitSurveyAnswers", "ParticipantFormId", "dbo.ParticipantForms");
            DropForeignKey("dbo.ParticipantExitSurveyAnswers", "ParticipantExitSurveyQuestionOptionId", "dbo.ParticipantExitSurveyQuestionOptions");
            DropForeignKey("dbo.ParticipantExitSurveyQuestionOptions", "ParticipantExitSurveyQuestionId", "dbo.ParticipantExitSurveyQuestions");
            DropIndex("dbo.ParticipantExitSurveyQuestionOptions", new[] { "ParticipantExitSurveyQuestionId" });
            DropIndex("dbo.ParticipantExitSurveyAnswers", new[] { "ParticipantFormId" });
            DropIndex("dbo.ParticipantExitSurveyAnswers", new[] { "ParticipantExitSurveyQuestionOptionId" });
            DropTable("dbo.ParticipantExitSurveyQuestions");
            DropTable("dbo.ParticipantExitSurveyQuestionOptions");
            DropTable("dbo.ParticipantExitSurveyAnswers");
        }
    }
}
