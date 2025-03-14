using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.17.02")]
	public partial class v021702 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attachments", "DocumentType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Attachments", "DocumentType");
        }
    }
}
