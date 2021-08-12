using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.06.01")]
	public partial class v020601 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.Attestations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        State = c.Int(nullable: false),
                        ClaimedCosts = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AllocatedCosts = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UnusedFunds = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AttestationNotApplicable = c.Boolean(),
                        GrantApplicationId = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GrantApplications", t => t.GrantApplicationId, cascadeDelete: true)
                .Index(t => t.GrantApplicationId);
            
            AddColumn("dbo.GrantApplications", "AttestationId", c => c.Int());
            CreateIndex("dbo.GrantApplications", "AttestationId");
            AddForeignKey("dbo.GrantApplications", "AttestationId", "dbo.Attestations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GrantApplications", "AttestationId", "dbo.Attestations");
            DropForeignKey("dbo.Attestations", "GrantApplicationId", "dbo.GrantApplications");
            DropIndex("dbo.Attestations", new[] { "GrantApplicationId" });
            DropIndex("dbo.GrantApplications", new[] { "AttestationId" });
            DropColumn("dbo.GrantApplications", "AttestationId");
            DropTable("dbo.Attestations");
        }
    }
}
