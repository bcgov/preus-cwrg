using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.18.00")]
    public partial class v021800 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttestationDocuments",
                c => new
                    {
                        AttestationId = c.Int(nullable: false),
                        AttachmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AttestationId, t.AttachmentId })
                .ForeignKey("dbo.Attestations", t => t.AttestationId)
                .ForeignKey("dbo.Attachments", t => t.AttachmentId)
                .Index(t => t.AttestationId)
                .Index(t => t.AttachmentId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttestationDocuments", "AttachmentId", "dbo.Attachments");
            DropForeignKey("dbo.AttestationDocuments", "AttestationId", "dbo.Attestations");
            DropIndex("dbo.AttestationDocuments", new[] { "AttachmentId" });
            DropIndex("dbo.AttestationDocuments", new[] { "AttestationId" });
            DropTable("dbo.AttestationDocuments");
        }
    }
}
