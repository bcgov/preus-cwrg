using System.ComponentModel;

namespace CJG.Infrastructure.EF.Migrations
{
	[Description("v02.13.02")]
	public partial class v021302 : ExtendedDbMigration
    {
		public override void Up()
        {
            CreateTable(
                "dbo.GrantApplicationEvaluationNotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NoteTypeId = c.Int(nullable: false),
                        GrantApplicationId = c.Int(nullable: false),
                        AttachmentId = c.Int(),
                        CreatorId = c.Int(),
                        Content = c.String(nullable: false),
                        DateAdded = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateUpdated = c.DateTime(precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attachments", t => t.AttachmentId)
                .ForeignKey("dbo.InternalUsers", t => t.CreatorId)
                .ForeignKey("dbo.GrantApplications", t => t.GrantApplicationId, cascadeDelete: true)
                .ForeignKey("dbo.NoteTypes", t => t.NoteTypeId, cascadeDelete: true)
                .Index(t => new { t.NoteTypeId, t.GrantApplicationId, t.CreatorId }, name: "IX_Note")
                .Index(t => t.AttachmentId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GrantApplicationEvaluationNotes", "NoteTypeId", "dbo.NoteTypes");
            DropForeignKey("dbo.GrantApplicationEvaluationNotes", "GrantApplicationId", "dbo.GrantApplications");
            DropForeignKey("dbo.GrantApplicationEvaluationNotes", "CreatorId", "dbo.InternalUsers");
            DropForeignKey("dbo.GrantApplicationEvaluationNotes", "AttachmentId", "dbo.Attachments");
            DropIndex("dbo.GrantApplicationEvaluationNotes", new[] { "AttachmentId" });
            DropIndex("dbo.GrantApplicationEvaluationNotes", "IX_Note");
            DropTable("dbo.GrantApplicationEvaluationNotes");
        }
    }
}
