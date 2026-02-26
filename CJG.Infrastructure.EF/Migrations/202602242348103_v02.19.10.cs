using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.19.10")]
	public partial class v021910 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.AttestationParticipants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttestationId = c.Int(nullable: false),
                        ParticipantFormId = c.Int(nullable: false),
                        ProgramInitiativeId = c.Int(),
                        TotalApprovedCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalAmountSpent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UnusedFunds = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attestations", t => t.AttestationId, cascadeDelete: true)
                .ForeignKey("dbo.ParticipantForms", t => t.ParticipantFormId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramInitiatives", t => t.ProgramInitiativeId)
                .Index(t => t.AttestationId)
                .Index(t => t.ParticipantFormId)
                .Index(t => t.ProgramInitiativeId);
            
            CreateTable(
                "dbo.AttestationParticipantCosts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttestationParticipantId = c.Int(nullable: false),
                        EligibleExpenseBreakdownId = c.Int(),
                        RequestedOtherDefinition = c.String(),
                        RequestedTotalSpent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ApprovedOtherDefinition = c.String(),
                        ApprovedTotalSpent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AttestationParticipants", t => t.AttestationParticipantId, cascadeDelete: true)
                .ForeignKey("dbo.EligibleExpenseBreakdowns", t => t.EligibleExpenseBreakdownId)
                .Index(t => t.AttestationParticipantId)
                .Index(t => t.EligibleExpenseBreakdownId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttestationParticipants", "ProgramInitiativeId", "dbo.ProgramInitiatives");
            DropForeignKey("dbo.AttestationParticipants", "ParticipantFormId", "dbo.ParticipantForms");
            DropForeignKey("dbo.AttestationParticipantCosts", "EligibleExpenseBreakdownId", "dbo.EligibleExpenseBreakdowns");
            DropForeignKey("dbo.AttestationParticipantCosts", "AttestationParticipantId", "dbo.AttestationParticipants");
            DropForeignKey("dbo.AttestationParticipants", "AttestationId", "dbo.Attestations");
            DropIndex("dbo.AttestationParticipantCosts", new[] { "EligibleExpenseBreakdownId" });
            DropIndex("dbo.AttestationParticipantCosts", new[] { "AttestationParticipantId" });
            DropIndex("dbo.AttestationParticipants", new[] { "ProgramInitiativeId" });
            DropIndex("dbo.AttestationParticipants", new[] { "ParticipantFormId" });
            DropIndex("dbo.AttestationParticipants", new[] { "AttestationId" });
            DropTable("dbo.AttestationParticipantCosts");
            DropTable("dbo.AttestationParticipants");
        }
    }
}
