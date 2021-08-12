using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.15.03")]
    public partial class v021503 : ExtendedDbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DirectorBudgets", "FiscalYearId", "dbo.FiscalYears");
            DropIndex("dbo.DirectorBudgets", new[] { "FiscalYearId" });
            AlterColumn("dbo.DirectorBudgets", "FiscalYearId", c => c.Int(nullable: false));
            CreateIndex("dbo.DirectorBudgets", "FiscalYearId");
            AddForeignKey("dbo.DirectorBudgets", "FiscalYearId", "dbo.FiscalYears", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DirectorBudgets", "FiscalYearId", "dbo.FiscalYears");
            DropIndex("dbo.DirectorBudgets", new[] { "FiscalYearId" });
            AlterColumn("dbo.DirectorBudgets", "FiscalYearId", c => c.Int());
            CreateIndex("dbo.DirectorBudgets", "FiscalYearId");
            AddForeignKey("dbo.DirectorBudgets", "FiscalYearId", "dbo.FiscalYears", "Id");
        }
    }
}
