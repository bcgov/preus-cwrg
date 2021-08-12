using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.11.00")]
	public partial class v021100 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.DirectorBudgets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BudgetEntryType = c.Int(nullable: false),
                        StreamFilter = c.String(),
                        Budget = c.Decimal(precision: 18, scale: 2),
                        WeeklyReceivablesNumber = c.Int(),
                        WeeklyReceivablesTotal = c.Decimal(precision: 18, scale: 2),
                        ForecastBudget = c.Decimal(precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);

			PostDeployment();
        }
        
        public override void Down()
        {
            DropTable("dbo.DirectorBudgets");
        }
    }
}
