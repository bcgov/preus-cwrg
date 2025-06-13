using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.18.03")]
    public partial class v021803 : ExtendedDbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GrantApplicationStateChanges", "Reason", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GrantApplicationStateChanges", "Reason", c => c.String(maxLength: 2000));
        }
    }
}
