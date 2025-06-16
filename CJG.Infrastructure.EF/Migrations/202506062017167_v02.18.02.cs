using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
    [Description("v02.18.02")]
    public partial class v021802 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GrantApplications", "PrimaryAssessorId", c => c.Int());
            CreateIndex("dbo.GrantApplications", "PrimaryAssessorId");
            AddForeignKey("dbo.GrantApplications", "PrimaryAssessorId", "dbo.InternalUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GrantApplications", "PrimaryAssessorId", "dbo.InternalUsers");
            DropIndex("dbo.GrantApplications", new[] { "PrimaryAssessorId" });
            DropColumn("dbo.GrantApplications", "PrimaryAssessorId");
        }
    }
}
