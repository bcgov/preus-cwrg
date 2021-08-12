using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.10.04")]
	public partial class v021004 : ExtendedDbMigration
    {
		public override void Up()
        {
            DropForeignKey("dbo.Attestations", "GrantApplicationId", "dbo.GrantApplications");
            DropIndex("dbo.Attestations", new[] { "GrantApplicationId" });
            DropColumn("dbo.Attestations", "GrantApplicationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Attestations", "GrantApplicationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Attestations", "GrantApplicationId");
            AddForeignKey("dbo.Attestations", "GrantApplicationId", "dbo.GrantApplications", "Id", cascadeDelete: true);
        }
    }
}
