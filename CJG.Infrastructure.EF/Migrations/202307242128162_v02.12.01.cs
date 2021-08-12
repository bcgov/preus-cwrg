using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.12.01")]
    public partial class v021201 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClaimEligibleCosts", "AccountsReceivableOverpayment", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ClaimEligibleCosts", "AccountsReceivablePaidDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClaimEligibleCosts", "AccountsReceivablePaidDate");
            DropColumn("dbo.ClaimEligibleCosts", "AccountsReceivableOverpayment");
        }
    }
}
