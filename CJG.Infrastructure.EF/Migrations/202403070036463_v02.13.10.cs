using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.13.10")]
    public partial class v021310 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GrantStreamEligibilityAnswers", "GrantWriterDesignation", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.GrantStreamEligibilityAnswers", "GrantWriterDesignation");
        }
    }
}
