using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.08.00")]
    public partial class v020800 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GrantApplicationTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrantApplicationId = c.Int(nullable: false),
                        ChecklistItemId = c.Int(nullable: false),
                        IsChecked = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChecklistItems", t => t.ChecklistItemId, cascadeDelete: true)
                .ForeignKey("dbo.GrantApplications", t => t.GrantApplicationId, cascadeDelete: true)
                .Index(t => t.GrantApplicationId)
                .Index(t => t.ChecklistItemId);
            
            CreateTable(
                "dbo.ChecklistItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChecklistCategoryId = c.Int(nullable: false),
                        Caption = c.String(nullable: false, maxLength: 250),
                        RowSequence = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChecklistCategories", t => t.ChecklistCategoryId, cascadeDelete: true)
                .Index(t => t.ChecklistCategoryId);
            
            CreateTable(
                "dbo.ChecklistCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrantStreamId = c.Int(nullable: false),
                        Caption = c.String(nullable: false, maxLength: 250),
                        RowSequence = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GrantStreams", t => t.GrantStreamId)
                .Index(t => t.GrantStreamId, name: "IX_GrantStreamChecklistCategories");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GrantApplicationTasks", "GrantApplicationId", "dbo.GrantApplications");
            DropForeignKey("dbo.GrantApplicationTasks", "ChecklistItemId", "dbo.ChecklistItems");
            DropForeignKey("dbo.ChecklistItems", "ChecklistCategoryId", "dbo.ChecklistCategories");
            DropForeignKey("dbo.ChecklistCategories", "GrantStreamId", "dbo.GrantStreams");
            DropIndex("dbo.ChecklistCategories", "IX_GrantStreamChecklistCategories");
            DropIndex("dbo.ChecklistItems", new[] { "ChecklistCategoryId" });
            DropIndex("dbo.GrantApplicationTasks", new[] { "ChecklistItemId" });
            DropIndex("dbo.GrantApplicationTasks", new[] { "GrantApplicationId" });
            DropTable("dbo.ChecklistCategories");
            DropTable("dbo.ChecklistItems");
            DropTable("dbo.GrantApplicationTasks");
        }
    }
}
