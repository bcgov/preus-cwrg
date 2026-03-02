using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.06")]
	public partial class v021906 : ExtendedDbMigration
    {
		public override void Up()
        {
            AddColumn("dbo.ParticipantForms", "HaveYouEverBeenEmployed", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantForms", "HaveYouEverBeenEmployed");
        }
    }
}
