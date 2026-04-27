using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.20.01")]
	public partial class v022001 : ExtendedDbMigration
    {
		public override void Up()
        {
            AddColumn("dbo.AccountsReceivableEntries", "OverpaymentLMDA", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.AccountsReceivableEntries", "OverpaymentWDA", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AccountsReceivableEntries", "OverpaymentWDA");
            DropColumn("dbo.AccountsReceivableEntries", "OverpaymentLMDA");
        }
    }
}
