using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.04.00")]
	public partial class v020400 : ExtendedDbMigration
	{
        public override void Up()
        {
            AddColumn("dbo.ProgramDescriptions", "PubliclyAvailableDescription", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramDescriptions", "PubliclyAvailableDescription");
        }
    }
}
