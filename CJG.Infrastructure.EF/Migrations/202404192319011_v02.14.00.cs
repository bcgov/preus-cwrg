using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.14.00")]
    public partial class v021400 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attachments", "AttachmentType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Attachments", "AttachmentType");
        }
    }
}
