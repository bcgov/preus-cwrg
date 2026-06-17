using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{

	[Description("v02.25.00")]
    public partial class v022500 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramInitiatives", "ShowInProgramFundingReport", c => c.Boolean(nullable: false));
			PostDeployment();
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramInitiatives", "ShowInProgramFundingReport");
        }
    }
}
