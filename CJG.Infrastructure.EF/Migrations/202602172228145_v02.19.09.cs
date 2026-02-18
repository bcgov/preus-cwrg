using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.09")]
    public partial class v021909 : ExtendedDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ParticipantFundingStreams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Caption = c.String(nullable: false, maxLength: 250),
                        IsActive = c.Boolean(nullable: false),
                        RowSequence = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Caption, unique: true)
                .Index(t => t.IsActive, name: "IX_Active");
            
            AddColumn("dbo.ParticipantForms", "AffectedByTariffs", c => c.Boolean());
            AddColumn("dbo.ParticipantForms", "ParticipantFundingStreamId", c => c.Int());
            CreateIndex("dbo.ParticipantForms", "ParticipantFundingStreamId");
            AddForeignKey("dbo.ParticipantForms", "ParticipantFundingStreamId", "dbo.ParticipantFundingStreams", "Id");

			PostDeployment();
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantForms", "ParticipantFundingStreamId", "dbo.ParticipantFundingStreams");
            DropIndex("dbo.ParticipantFundingStreams", "IX_Active");
            DropIndex("dbo.ParticipantFundingStreams", new[] { "Caption" });
            DropIndex("dbo.ParticipantForms", new[] { "ParticipantFundingStreamId" });
            DropColumn("dbo.ParticipantForms", "ParticipantFundingStreamId");
            DropColumn("dbo.ParticipantForms", "AffectedByTariffs");
            DropTable("dbo.ParticipantFundingStreams");
        }
    }
}
