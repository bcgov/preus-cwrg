using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.10.03")]
    public partial class v021003 : ExtendedDbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProofOfPayments", "GrantApplicationId", "dbo.GrantApplications");
            DropIndex("dbo.ProofOfPayments", new[] { "GrantApplicationId" });
            DropColumn("dbo.ProofOfPayments", "GrantApplicationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProofOfPayments", "GrantApplicationId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProofOfPayments", "GrantApplicationId");
            AddForeignKey("dbo.ProofOfPayments", "GrantApplicationId", "dbo.GrantApplications", "Id", cascadeDelete: true);
        }
    }
}
