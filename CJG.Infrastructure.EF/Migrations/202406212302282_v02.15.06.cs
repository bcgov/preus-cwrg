using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.15.06")]
	public partial class v021506 : ExtendedDbMigration
	{
        public override void Up()
        {
            CreateTable(
                "dbo.ParticipantWithdrawalSurveyAnswers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParticipantWithdrawalSurveyQuestionOptionId = c.Int(nullable: false),
                        ParticipantFormId = c.Int(nullable: false),
                        OptionTextDisplayed = c.String(),
                        Answer = c.Boolean(nullable: false),
                        TextAnswer = c.String(),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantForms", t => t.ParticipantFormId, cascadeDelete: true)
                .ForeignKey("dbo.ParticipantWithdrawalSurveyQuestionOptions", t => t.ParticipantWithdrawalSurveyQuestionOptionId, cascadeDelete: true)
                .Index(t => t.ParticipantWithdrawalSurveyQuestionOptionId)
                .Index(t => t.ParticipantFormId);
            
            CreateTable(
                "dbo.ParticipantWithdrawalSurveyQuestionOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParticipantWithdrawalSurveyQuestionId = c.Int(nullable: false),
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
                .ForeignKey("dbo.ParticipantWithdrawalSurveyQuestions", t => t.ParticipantWithdrawalSurveyQuestionId, cascadeDelete: true)
                .Index(t => t.ParticipantWithdrawalSurveyQuestionId);
            
            CreateTable(
                "dbo.ParticipantWithdrawalSurveyQuestions",
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
            
            AddColumn("dbo.ParticipantForms", "WithdrawalKey", c => c.Guid());
            AddColumn("dbo.ParticipantForms", "TrainingWithdrawalDate", c => c.DateTime(precision: 7, storeType: "datetime2"));

			PostDeployment();
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantWithdrawalSurveyAnswers", "ParticipantWithdrawalSurveyQuestionOptionId", "dbo.ParticipantWithdrawalSurveyQuestionOptions");
            DropForeignKey("dbo.ParticipantWithdrawalSurveyQuestionOptions", "ParticipantWithdrawalSurveyQuestionId", "dbo.ParticipantWithdrawalSurveyQuestions");
            DropForeignKey("dbo.ParticipantWithdrawalSurveyAnswers", "ParticipantFormId", "dbo.ParticipantForms");
            DropIndex("dbo.ParticipantWithdrawalSurveyQuestionOptions", new[] { "ParticipantWithdrawalSurveyQuestionId" });
            DropIndex("dbo.ParticipantWithdrawalSurveyAnswers", new[] { "ParticipantFormId" });
            DropIndex("dbo.ParticipantWithdrawalSurveyAnswers", new[] { "ParticipantWithdrawalSurveyQuestionOptionId" });
            DropColumn("dbo.ParticipantForms", "TrainingWithdrawalDate");
            DropColumn("dbo.ParticipantForms", "WithdrawalKey");
            DropTable("dbo.ParticipantWithdrawalSurveyQuestions");
            DropTable("dbo.ParticipantWithdrawalSurveyQuestionOptions");
            DropTable("dbo.ParticipantWithdrawalSurveyAnswers");
        }
    }
}
