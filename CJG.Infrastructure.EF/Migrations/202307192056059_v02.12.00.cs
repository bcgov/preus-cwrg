using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.12.00")]
    public partial class v021200 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GlobalProgramBudgets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IntakeBudget = c.Decimal(precision: 18, scale: 2),
                        TrainingPeriodId = c.Int(nullable: false),
                        GrantStreamId = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.GlobalProgramBudgets");
        }
    }
}
