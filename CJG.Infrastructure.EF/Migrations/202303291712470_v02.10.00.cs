using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.10.00")]
    public partial class v021000 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrganizationDocuments",
                c => new
                    {
                        OrganizationId = c.Int(nullable: false),
                        AttachmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.OrganizationId, t.AttachmentId })
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .ForeignKey("dbo.Attachments", t => t.AttachmentId)
                .Index(t => t.OrganizationId)
                .Index(t => t.AttachmentId);
            
            CreateTable(
                "dbo.TrainingProviderInventoryDocuments",
                c => new
                    {
                        TrainingProviderInventoryId = c.Int(nullable: false),
                        AttachmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TrainingProviderInventoryId, t.AttachmentId })
                .ForeignKey("dbo.TrainingProviderInventory", t => t.TrainingProviderInventoryId)
                .ForeignKey("dbo.Attachments", t => t.AttachmentId)
                .Index(t => t.TrainingProviderInventoryId)
                .Index(t => t.AttachmentId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingProviderInventoryDocuments", "AttachmentId", "dbo.Attachments");
            DropForeignKey("dbo.TrainingProviderInventoryDocuments", "TrainingProviderInventoryId", "dbo.TrainingProviderInventory");
            DropForeignKey("dbo.OrganizationDocuments", "AttachmentId", "dbo.Attachments");
            DropForeignKey("dbo.OrganizationDocuments", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.TrainingProviderInventoryDocuments", new[] { "AttachmentId" });
            DropIndex("dbo.TrainingProviderInventoryDocuments", new[] { "TrainingProviderInventoryId" });
            DropIndex("dbo.OrganizationDocuments", new[] { "AttachmentId" });
            DropIndex("dbo.OrganizationDocuments", new[] { "OrganizationId" });
            DropTable("dbo.TrainingProviderInventoryDocuments");
            DropTable("dbo.OrganizationDocuments");
        }
    }
}
