using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.08.03")]
    public partial class v020803 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GrantApplicationEvaluationAnswers", "RowSequence", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GrantApplicationEvaluationAnswers", "RowSequence");
        }
    }
}
