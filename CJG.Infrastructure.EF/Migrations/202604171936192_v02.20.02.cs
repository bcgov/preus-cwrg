using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.20.02")]
    public partial class v022002 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClaimPayments",
                c => new
                    {
                        ClaimId = c.Int(nullable: false),
                        ClaimVersion = c.Int(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                        PaidLMDA = c.Decimal(precision: 18, scale: 2),
                        PaidWDA = c.Decimal(precision: 18, scale: 2),
                        LMDATariffTRSW = c.Boolean(nullable: false),
                        LMDATariffTRST = c.Boolean(nullable: false),
                        LMDATariffTRCO = c.Boolean(nullable: false),
                        WDATariffCWRG = c.Boolean(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Claims", t => new { t.ClaimId, t.ClaimVersion }, cascadeDelete: true)
                .Index(t => new { t.ClaimId, t.ClaimVersion });
            
            CreateTable(
                "dbo.ProgramFundingBudgetEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramFundingBudgetId = c.Int(nullable: false),
                        ProgramFundingBudgetRowId = c.Int(nullable: false),
                        Budget = c.Decimal(precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramFundingBudgets", t => t.ProgramFundingBudgetId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramFundingBudgetRows", t => t.ProgramFundingBudgetRowId, cascadeDelete: true)
                .Index(t => t.ProgramFundingBudgetId)
                .Index(t => t.ProgramFundingBudgetRowId);
            
            CreateTable(
                "dbo.ProgramFundingBudgets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FiscalYearId = c.Int(nullable: false),
                        StreamFilter = c.String(),
                        ProgramInitiativeId = c.Int(),
                        Budget = c.Decimal(precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FiscalYears", t => t.FiscalYearId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramInitiatives", t => t.ProgramInitiativeId)
                .Index(t => t.FiscalYearId)
                .Index(t => t.ProgramInitiativeId);
            
            CreateTable(
                "dbo.ProgramFundingBudgetRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntryType = c.Int(nullable: false),
                        FiscalYearId = c.Int(nullable: false),
                        Name = c.String(),
                        Sequence = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FiscalYears", t => t.FiscalYearId)
                .Index(t => t.FiscalYearId);
            
            AddColumn("dbo.Claims", "ClaimPaymentId", c => c.Int());
            CreateIndex("dbo.Claims", "ClaimPaymentId");
            AddForeignKey("dbo.Claims", "ClaimPaymentId", "dbo.ClaimPayments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramFundingBudgetEntries", "ProgramFundingBudgetRowId", "dbo.ProgramFundingBudgetRows");
            DropForeignKey("dbo.ProgramFundingBudgetRows", "FiscalYearId", "dbo.FiscalYears");
            DropForeignKey("dbo.ProgramFundingBudgets", "ProgramInitiativeId", "dbo.ProgramInitiatives");
            DropForeignKey("dbo.ProgramFundingBudgets", "FiscalYearId", "dbo.FiscalYears");
            DropForeignKey("dbo.ProgramFundingBudgetEntries", "ProgramFundingBudgetId", "dbo.ProgramFundingBudgets");
            DropForeignKey("dbo.Claims", "ClaimPaymentId", "dbo.ClaimPayments");
            DropForeignKey("dbo.ClaimPayments", new[] { "ClaimId", "ClaimVersion" }, "dbo.Claims");
            DropIndex("dbo.ProgramFundingBudgetRows", new[] { "FiscalYearId" });
            DropIndex("dbo.ProgramFundingBudgets", new[] { "ProgramInitiativeId" });
            DropIndex("dbo.ProgramFundingBudgets", new[] { "FiscalYearId" });
            DropIndex("dbo.ProgramFundingBudgetEntries", new[] { "ProgramFundingBudgetRowId" });
            DropIndex("dbo.ProgramFundingBudgetEntries", new[] { "ProgramFundingBudgetId" });
            DropIndex("dbo.ClaimPayments", new[] { "ClaimId", "ClaimVersion" });
            DropIndex("dbo.Claims", new[] { "ClaimPaymentId" });
            DropColumn("dbo.Claims", "ClaimPaymentId");
            DropTable("dbo.ProgramFundingBudgetRows");
            DropTable("dbo.ProgramFundingBudgets");
            DropTable("dbo.ProgramFundingBudgetEntries");
            DropTable("dbo.ClaimPayments");
        }
    }
}
