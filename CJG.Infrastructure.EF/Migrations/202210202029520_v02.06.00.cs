using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.06.00")]
	public partial class v020600 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.ProofOfPayments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        State = c.Int(nullable: false),
                        ProofNotApplicable = c.Boolean(),
                        GrantApplicationId = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GrantApplications", t => t.GrantApplicationId, cascadeDelete: true)
                .Index(t => t.GrantApplicationId);
            
            CreateTable(
                "dbo.ProofOfPaymentDocuments",
                c => new
                    {
                        ProofOfPaymentId = c.Int(nullable: false),
                        AttachmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProofOfPaymentId, t.AttachmentId })
                .ForeignKey("dbo.ProofOfPayments", t => t.ProofOfPaymentId)
                .ForeignKey("dbo.Attachments", t => t.AttachmentId)
                .Index(t => t.ProofOfPaymentId)
                .Index(t => t.AttachmentId);
            
            AddColumn("dbo.GrantApplications", "ProofOfPaymentId", c => c.Int());
            CreateIndex("dbo.GrantApplications", "ProofOfPaymentId");
            AddForeignKey("dbo.GrantApplications", "ProofOfPaymentId", "dbo.ProofOfPayments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GrantApplications", "ProofOfPaymentId", "dbo.ProofOfPayments");
            DropForeignKey("dbo.ProofOfPayments", "GrantApplicationId", "dbo.GrantApplications");
            DropForeignKey("dbo.ProofOfPaymentDocuments", "AttachmentId", "dbo.Attachments");
            DropForeignKey("dbo.ProofOfPaymentDocuments", "ProofOfPaymentId", "dbo.ProofOfPayments");
            DropIndex("dbo.ProofOfPaymentDocuments", new[] { "AttachmentId" });
            DropIndex("dbo.ProofOfPaymentDocuments", new[] { "ProofOfPaymentId" });
            DropIndex("dbo.ProofOfPayments", new[] { "GrantApplicationId" });
            DropIndex("dbo.GrantApplications", new[] { "ProofOfPaymentId" });
            DropColumn("dbo.GrantApplications", "ProofOfPaymentId");
            DropTable("dbo.ProofOfPaymentDocuments");
            DropTable("dbo.ProofOfPayments");
        }
    }
}
