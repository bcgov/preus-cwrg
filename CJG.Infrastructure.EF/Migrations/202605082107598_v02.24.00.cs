using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.24.00")]
	public partial class v022400 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaymentRequestAccountCodes",
                c => new
                    {
                        PaymentRequestBatchId = c.Int(nullable: false),
                        ClaimId = c.Int(nullable: false),
                        ClaimVersion = c.Int(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                        GLClientNumber = c.String(nullable: false, maxLength: 50),
                        GLRESP = c.String(nullable: false, maxLength: 20),
                        GLServiceLine = c.String(nullable: false, maxLength: 20),
                        GLSTOB = c.String(nullable: false, maxLength: 20),
                        GLProjectCode = c.String(nullable: false, maxLength: 20),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Claims", t => new { t.ClaimId, t.ClaimVersion })
                .ForeignKey("dbo.PaymentRequestBatches", t => t.PaymentRequestBatchId)
                .ForeignKey("dbo.PaymentRequests", t => new { t.PaymentRequestBatchId, t.ClaimId, t.ClaimVersion }, cascadeDelete: true)
                .Index(t => t.PaymentRequestBatchId)
                .Index(t => new { t.PaymentRequestBatchId, t.ClaimId, t.ClaimVersion })
                .Index(t => new { t.ClaimId, t.ClaimVersion });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PaymentRequestAccountCodes", new[] { "PaymentRequestBatchId", "ClaimId", "ClaimVersion" }, "dbo.PaymentRequests");
            DropForeignKey("dbo.PaymentRequestAccountCodes", "PaymentRequestBatchId", "dbo.PaymentRequestBatches");
            DropForeignKey("dbo.PaymentRequestAccountCodes", new[] { "ClaimId", "ClaimVersion" }, "dbo.Claims");
            DropIndex("dbo.PaymentRequestAccountCodes", new[] { "ClaimId", "ClaimVersion" });
            DropIndex("dbo.PaymentRequestAccountCodes", new[] { "PaymentRequestBatchId", "ClaimId", "ClaimVersion" });
            DropIndex("dbo.PaymentRequestAccountCodes", new[] { "PaymentRequestBatchId" });
            DropTable("dbo.PaymentRequestAccountCodes");
        }
    }
}
