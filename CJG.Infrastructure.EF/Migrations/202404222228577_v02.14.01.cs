using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.14.01")]
	public partial class v021401 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.SuccessStories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrantApplicationId = c.Int(nullable: false),
                        State = c.Int(nullable: false),
                        SuccessfulOutcome = c.Boolean(nullable: false),
                        NoOutcomeReason = c.String(),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GrantApplications", t => t.GrantApplicationId, cascadeDelete: true)
                .Index(t => t.GrantApplicationId);
            
            CreateTable(
                "dbo.SuccessStoryDocuments",
                c => new
                    {
                        SuccessStoryId = c.Int(nullable: false),
                        AttachmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SuccessStoryId, t.AttachmentId })
                .ForeignKey("dbo.SuccessStories", t => t.SuccessStoryId)
                .ForeignKey("dbo.Attachments", t => t.AttachmentId)
                .Index(t => t.SuccessStoryId)
                .Index(t => t.AttachmentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SuccessStories", "GrantApplicationId", "dbo.GrantApplications");
            DropForeignKey("dbo.SuccessStoryDocuments", "AttachmentId", "dbo.Attachments");
            DropForeignKey("dbo.SuccessStoryDocuments", "SuccessStoryId", "dbo.SuccessStories");
            DropIndex("dbo.SuccessStoryDocuments", new[] { "AttachmentId" });
            DropIndex("dbo.SuccessStoryDocuments", new[] { "SuccessStoryId" });
            DropIndex("dbo.SuccessStories", new[] { "GrantApplicationId" });
            DropTable("dbo.SuccessStoryDocuments");
            DropTable("dbo.SuccessStories");
        }
    }
}
