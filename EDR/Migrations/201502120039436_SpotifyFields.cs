namespace EDR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SpotifyFields : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalEvents",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        MediaSource = c.Int(nullable: false),
                        Url = c.String(),
                        Name = c.String(),
                        Event_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .Index(t => t.Event_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalEvents", "Event_Id", "dbo.Events");
            DropIndex("dbo.ExternalEvents", new[] { "Event_Id" });
            DropTable("dbo.ExternalEvents");
        }
    }
}
