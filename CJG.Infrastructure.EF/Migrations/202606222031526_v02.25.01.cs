using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.25.01")]
	public partial class v022501 : ExtendedDbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttestationParticipants", "ParticipantFundingStreamId", c => c.Int());
            CreateIndex("dbo.AttestationParticipants", "ParticipantFundingStreamId");
            AddForeignKey("dbo.AttestationParticipants", "ParticipantFundingStreamId", "dbo.ParticipantFundingStreams", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttestationParticipants", "ParticipantFundingStreamId", "dbo.ParticipantFundingStreams");
            DropIndex("dbo.AttestationParticipants", new[] { "ParticipantFundingStreamId" });
            DropColumn("dbo.AttestationParticipants", "ParticipantFundingStreamId");
        }
    }
}
