using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.13.01")]
    public partial class v021301 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClaimEvaluations",
                c => new
                    {
                        ClaimId = c.Int(nullable: false),
                        ClaimVersion = c.Int(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Claims", t => new { t.ClaimId, t.ClaimVersion }, cascadeDelete: true)
                .Index(t => new { t.ClaimId, t.ClaimVersion });
            
            CreateTable(
                "dbo.ClaimEvaluationAnswers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimEvaluationId = c.Int(nullable: false),
                        ClaimEvaluationFormQuestionReferenceId = c.Int(nullable: false),
                        QuestionType = c.Int(nullable: false),
                        QuestionAsked = c.String(),
                        AnswerGiven = c.Int(nullable: false),
                        RowSequence = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ClaimEvaluations", t => t.ClaimEvaluationId, cascadeDelete: true)
                .Index(t => t.ClaimEvaluationId);
            
            CreateTable(
                "dbo.ClaimEvaluationFormQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        ClaimEvaluationFormQuestionType = c.Int(nullable: false),
                        RowSequence = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Claims", "ClaimEvaluationId", c => c.Int());
            CreateIndex("dbo.Claims", "ClaimEvaluationId");
            AddForeignKey("dbo.Claims", "ClaimEvaluationId", "dbo.ClaimEvaluations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Claims", "ClaimEvaluationId", "dbo.ClaimEvaluations");
            DropForeignKey("dbo.ClaimEvaluationAnswers", "ClaimEvaluationId", "dbo.ClaimEvaluations");
            DropForeignKey("dbo.ClaimEvaluations", new[] { "ClaimId", "ClaimVersion" }, "dbo.Claims");
            DropIndex("dbo.ClaimEvaluationAnswers", new[] { "ClaimEvaluationId" });
            DropIndex("dbo.ClaimEvaluations", new[] { "ClaimId", "ClaimVersion" });
            DropIndex("dbo.Claims", new[] { "ClaimEvaluationId" });
            DropColumn("dbo.Claims", "ClaimEvaluationId");
            DropTable("dbo.ClaimEvaluationFormQuestions");
            DropTable("dbo.ClaimEvaluationAnswers");
            DropTable("dbo.ClaimEvaluations");
        }
    }
}
