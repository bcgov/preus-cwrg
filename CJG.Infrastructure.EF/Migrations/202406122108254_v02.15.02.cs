using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.15.02")]
	public partial class v021502 : ExtendedDbMigration
    {
		public override void Up()
        {
            AddColumn("dbo.DirectorBudgets", "FiscalYearId", c => c.Int());
            CreateIndex("dbo.DirectorBudgets", "FiscalYearId");
            AddForeignKey("dbo.DirectorBudgets", "FiscalYearId", "dbo.FiscalYears", "Id");

            PostDeployment();
        }

		public override void Down()
        {
            DropForeignKey("dbo.DirectorBudgets", "FiscalYearId", "dbo.FiscalYears");
            DropIndex("dbo.DirectorBudgets", new[] { "FiscalYearId" });
            DropColumn("dbo.DirectorBudgets", "FiscalYearId");
        }
    }
}