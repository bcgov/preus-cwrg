using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.03")]
	public partial class v020403 : ExtendedDbMigration
	{
        public override void Up()
        {
            AlterColumn("dbo.GrantStreams", "AttachmentsUserGuidance", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GrantStreams", "AttachmentsUserGuidance", c => c.String(maxLength: 2500));
        }
    }
}
