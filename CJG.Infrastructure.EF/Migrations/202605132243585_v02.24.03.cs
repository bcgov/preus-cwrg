using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.24.03")]    
    public partial class v022403 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attestations", "ReviewedByAdjudicatorAnalyst", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Attestations", "ReviewedByAdjudicatorAnalyst");
        }
    }
}
