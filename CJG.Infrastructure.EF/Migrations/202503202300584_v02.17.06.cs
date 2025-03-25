using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.17.06")]
	public partial class v021706 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.DirectorBudgetEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DirectorBudgetId = c.Int(nullable: false),
                        DirectorBudgetRowId = c.Int(nullable: false),
                        Budget = c.Decimal(precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DirectorBudgets", t => t.DirectorBudgetId, cascadeDelete: true)
                .ForeignKey("dbo.DirectorBudgetRows", t => t.DirectorBudgetRowId, cascadeDelete: true)
                .Index(t => t.DirectorBudgetId)
                .Index(t => t.DirectorBudgetRowId);
            
            CreateTable(
                "dbo.DirectorBudgetRows",
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
            
            AddColumn("dbo.DirectorBudgets", "ProgramInitiativeId", c => c.Int());
            CreateIndex("dbo.DirectorBudgets", "ProgramInitiativeId");
            AddForeignKey("dbo.DirectorBudgets", "ProgramInitiativeId", "dbo.ProgramInitiatives", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DirectorBudgetEntries", "DirectorBudgetRowId", "dbo.DirectorBudgetRows");
            DropForeignKey("dbo.DirectorBudgetRows", "FiscalYearId", "dbo.FiscalYears");
            DropForeignKey("dbo.DirectorBudgets", "ProgramInitiativeId", "dbo.ProgramInitiatives");
            DropForeignKey("dbo.DirectorBudgetEntries", "DirectorBudgetId", "dbo.DirectorBudgets");
            DropIndex("dbo.DirectorBudgetRows", new[] { "FiscalYearId" });
            DropIndex("dbo.DirectorBudgets", new[] { "ProgramInitiativeId" });
            DropIndex("dbo.DirectorBudgetEntries", new[] { "DirectorBudgetRowId" });
            DropIndex("dbo.DirectorBudgetEntries", new[] { "DirectorBudgetId" });
            DropColumn("dbo.DirectorBudgets", "ProgramInitiativeId");
            DropTable("dbo.DirectorBudgetRows");
            DropTable("dbo.DirectorBudgetEntries");
        }
    }
}
