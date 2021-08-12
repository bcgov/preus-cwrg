using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.08.01")]
    public partial class v020801 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GrantApplicationEvaluations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrantApplicationId = c.Int(nullable: false),
                        HighLevelRationale = c.String(),
                        ApplicationNotes = c.String(),
                        EvaluationStatus = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GrantApplications", t => t.GrantApplicationId, cascadeDelete: true)
                .Index(t => t.GrantApplicationId);
            
            CreateTable(
                "dbo.GrantApplicationEvaluationAnswers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrantApplicationEvaluationId = c.Int(nullable: false),
                        EvaluationFormQuestionReferenceId = c.Int(nullable: false),
                        QuestionType = c.Int(nullable: false),
                        QuestionAsked = c.String(),
                        AnswerGiven = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GrantApplicationEvaluations", t => t.GrantApplicationEvaluationId, cascadeDelete: true)
                .Index(t => t.GrantApplicationEvaluationId);
            
            CreateTable(
                "dbo.EvaluationFormQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        EvaluationFormQuestionType = c.Int(nullable: false),
                        RowSequence = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EvaluationFormResources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RowSequence = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Attachment_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attachments", t => t.Attachment_Id)
                .Index(t => t.Attachment_Id);
            
            AddColumn("dbo.GrantApplications", "GrantApplicationEvaluationId", c => c.Int());
            AddColumn("dbo.GrantStreams", "Intent", c => c.String());
            CreateIndex("dbo.GrantApplications", "GrantApplicationEvaluationId");
            AddForeignKey("dbo.GrantApplications", "GrantApplicationEvaluationId", "dbo.GrantApplicationEvaluations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EvaluationFormResources", "Attachment_Id", "dbo.Attachments");
            DropForeignKey("dbo.GrantApplications", "GrantApplicationEvaluationId", "dbo.GrantApplicationEvaluations");
            DropForeignKey("dbo.GrantApplicationEvaluations", "GrantApplicationId", "dbo.GrantApplications");
            DropForeignKey("dbo.GrantApplicationEvaluationAnswers", "GrantApplicationEvaluationId", "dbo.GrantApplicationEvaluations");
            DropIndex("dbo.EvaluationFormResources", new[] { "Attachment_Id" });
            DropIndex("dbo.GrantApplicationEvaluationAnswers", new[] { "GrantApplicationEvaluationId" });
            DropIndex("dbo.GrantApplicationEvaluations", new[] { "GrantApplicationId" });
            DropIndex("dbo.GrantApplications", new[] { "GrantApplicationEvaluationId" });
            DropColumn("dbo.GrantStreams", "Intent");
            DropColumn("dbo.GrantApplications", "GrantApplicationEvaluationId");
            DropTable("dbo.EvaluationFormResources");
            DropTable("dbo.EvaluationFormQuestions");
            DropTable("dbo.GrantApplicationEvaluationAnswers");
            DropTable("dbo.GrantApplicationEvaluations");
        }
    }
}
