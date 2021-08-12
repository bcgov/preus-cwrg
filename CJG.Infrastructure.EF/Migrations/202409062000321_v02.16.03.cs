using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.16.03")]
    public partial class v021603 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccountsReceivableEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountsReceivableId = c.Int(nullable: false),
                        ServiceCategoryId = c.Int(nullable: false),
                        Overpayment = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccountsReceivables", t => t.AccountsReceivableId, cascadeDelete: true)
                .ForeignKey("dbo.ServiceCategories", t => t.ServiceCategoryId, cascadeDelete: true)
                .Index(t => t.AccountsReceivableId)
                .Index(t => t.ServiceCategoryId);
            
            CreateTable(
                "dbo.AccountsReceivables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrantApplicationId = c.Int(nullable: false),
                        PaidDate = c.DateTime(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GrantApplications", t => t.GrantApplicationId, cascadeDelete: true)
                .Index(t => t.GrantApplicationId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AccountsReceivableEntries", "ServiceCategoryId", "dbo.ServiceCategories");
            DropForeignKey("dbo.AccountsReceivableEntries", "AccountsReceivableId", "dbo.AccountsReceivables");
            DropForeignKey("dbo.AccountsReceivables", "GrantApplicationId", "dbo.GrantApplications");
            DropIndex("dbo.AccountsReceivables", new[] { "GrantApplicationId" });
            DropIndex("dbo.AccountsReceivableEntries", new[] { "ServiceCategoryId" });
            DropIndex("dbo.AccountsReceivableEntries", new[] { "AccountsReceivableId" });
            DropTable("dbo.AccountsReceivables");
            DropTable("dbo.AccountsReceivableEntries");
        }
    }
}
