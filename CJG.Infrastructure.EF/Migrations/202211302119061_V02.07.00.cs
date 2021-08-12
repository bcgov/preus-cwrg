using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.07.00")]
	public partial class V020700 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GrantApplications", "ReturnedToDraft", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GrantApplications", "ReturnedToDraft");
        }
    }
}
