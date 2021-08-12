using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.09.00")]
    public partial class v020900 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NationalOccupationalClassifications", "NOCVersion", c => c.String());

			PostDeployment();
        }
        
        public override void Down()
        {
            DropColumn("dbo.NationalOccupationalClassifications", "NOCVersion");
        }
    }
}
