namespace CJG.Infrastructure.EF.Migrations
{
    using System;
	using System.ComponentModel;
	using System.Data.Entity.Migrations;

	[Description("v02.04.09")]
	public partial class v020409 : ExtendedDbMigration
	{
        public override void Up()
        {
            AddColumn("dbo.TrainingPrograms", "ShortTermOccupationalCert", c => c.Boolean());
            AddColumn("dbo.TrainingPrograms", "OnTheJobTraining", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingPrograms", "OnTheJobTraining");
            DropColumn("dbo.TrainingPrograms", "ShortTermOccupationalCert");
        }
    }
}
