using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("V02.17.03")]
	public partial class v021703 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GrantApplications", "NotRequestingESS", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.GrantApplications", "NotRequestingESS");
        }
    }
}
