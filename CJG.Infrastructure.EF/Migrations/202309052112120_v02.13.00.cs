using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.13.00")]
    public partial class v021300 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GrantStreamEligibilityQuestions", "CollectContactInformation", c => c.Boolean(nullable: false));
            AddColumn("dbo.GrantStreamEligibilityAnswers", "ContactName", c => c.String(maxLength: 500));
            AddColumn("dbo.GrantStreamEligibilityAnswers", "ContactEmailAddress", c => c.String(maxLength: 500));
            AddColumn("dbo.GrantStreamEligibilityAnswers", "ContactPhoneNumber", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GrantStreamEligibilityAnswers", "ContactPhoneNumber");
            DropColumn("dbo.GrantStreamEligibilityAnswers", "ContactEmailAddress");
            DropColumn("dbo.GrantStreamEligibilityAnswers", "ContactName");
            DropColumn("dbo.GrantStreamEligibilityQuestions", "CollectContactInformation");
        }
    }
}
