using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.17.04")]
	public partial class v021704 : ExtendedDbMigration
	{
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramInitiatives",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 255),
                        Code = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        RowSequence = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.IsActive, name: "IX_Active");
            
            AddColumn("dbo.GrantApplications", "ProgramInitiativeId", c => c.Int());
            CreateIndex("dbo.GrantApplications", "ProgramInitiativeId");
            AddForeignKey("dbo.GrantApplications", "ProgramInitiativeId", "dbo.ProgramInitiatives", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GrantApplications", "ProgramInitiativeId", "dbo.ProgramInitiatives");
            DropIndex("dbo.ProgramInitiatives", "IX_Active");
            DropIndex("dbo.GrantApplications", new[] { "ProgramInitiativeId" });
            DropColumn("dbo.GrantApplications", "ProgramInitiativeId");
            DropTable("dbo.ProgramInitiatives");
        }
    }
}
