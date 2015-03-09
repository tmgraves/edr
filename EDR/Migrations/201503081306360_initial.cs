namespace EDR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClassTeacherInvitations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClassId = c.Int(nullable: false),
                        TeacherId = c.Int(nullable: false),
                        Approved = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Classes", t => t.ClassId)
                .ForeignKey("dbo.Teachers", t => t.TeacherId)
                .Index(t => new { t.ClassId, t.TeacherId }, unique: true, name: "IX_ClassTeacherInvitation");
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        FacebookLink = c.String(),
                        FacebookId = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        AllDay = c.Boolean(nullable: false),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        Duration = c.Int(nullable: false),
                        EndDate = c.DateTime(),
                        Price = c.Decimal(precision: 18, scale: 2),
                        IsAvailable = c.Boolean(nullable: false),
                        Recurring = c.Boolean(nullable: false),
                        Frequency = c.Int(nullable: false),
                        Interval = c.Int(nullable: false),
                        MonthDays = c.String(),
                        PhotoUrl = c.String(),
                        VideoUrl = c.String(),
                        Creator_Id = c.String(maxLength: 128),
                        Place_Id = c.Int(),
                        Series_Id = c.Int(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Creator_Id)
                .ForeignKey("dbo.Places", t => t.Place_Id)
                .ForeignKey("dbo.Series", t => t.Series_Id)
                .ForeignKey("dbo.Users", t => t.ApplicationUser_Id)
                .Index(t => t.Creator_Id)
                .Index(t => t.Place_Id)
                .Index(t => t.Series_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        ZipCode = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        StartDate = c.DateTime(),
                        FacebookUsername = c.String(),
                        FacebookToken = c.String(),
                        YouTubeUsername = c.String(),
                        InstagramId = c.Int(),
                        InstagramUsername = c.String(),
                        InstagramToken = c.String(),
                        SpotifyUri = c.String(),
                        SpotifyId = c.String(),
                        SpotifyUsername = c.String(),
                        SpotifyToken = c.String(),
                        SpotifyRefreshToken = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Event_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .Index(t => t.Event_Id);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.DanceStyles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        YouTubeVideoID = c.String(),
                        PhotoUrl = c.String(),
                        SpotifyPlaylist = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "NameIndex");
            
            CreateTable(
                "dbo.EventMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        EventId = c.Int(nullable: false),
                        Joined = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.EventId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => new { t.UserId, t.EventId }, unique: true, name: "IX_EventMembers");
            
            CreateTable(
                "dbo.LinkedFacebookObjects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FacebookId = c.String(),
                        MediaSource = c.Int(nullable: false),
                        Url = c.String(),
                        Name = c.String(),
                        ObjectType = c.Int(nullable: false),
                        Event_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .Index(t => t.Event_Id);
            
            CreateTable(
                "dbo.Pictures",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Filename = c.String(),
                        ThumbnailFilename = c.String(),
                        PhotoDate = c.DateTime(nullable: false),
                        SourceLink = c.String(),
                        MediaSource = c.Int(nullable: false),
                        FacebookId = c.String(),
                        InstagramId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Places",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Address = c.String(),
                        Address2 = c.String(),
                        City = c.String(),
                        State = c.Int(nullable: false),
                        Zip = c.String(),
                        Country = c.String(),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        FacebookLink = c.String(),
                        Website = c.String(),
                        Filename = c.String(),
                        ThumbnailFilename = c.String(),
                        PlaceType = c.Int(nullable: false),
                        FacebookId = c.String(),
                        Selected = c.Boolean(),
                        Discriminator = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Owners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContactEmail = c.String(),
                        Website = c.String(),
                        Facebook = c.String(),
                        Approved = c.Boolean(),
                        ApproveDate = c.DateTime(),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.PlaceOwnerInvitations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PlaceId = c.Int(nullable: false),
                        OwnerId = c.Int(nullable: false),
                        Approved = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Owners", t => t.OwnerId)
                .ForeignKey("dbo.Places", t => t.PlaceId)
                .Index(t => new { t.PlaceId, t.OwnerId }, unique: true, name: "IX_PlaceOwnerInvitation");
            
            CreateTable(
                "dbo.Promoters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContactEmail = c.String(),
                        Facebook = c.String(),
                        Website = c.String(),
                        Approved = c.Boolean(),
                        ApproveDate = c.DateTime(),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.SocialPromoterInvitations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SocialId = c.Int(nullable: false),
                        PromoterId = c.Int(nullable: false),
                        Approved = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teachers", t => t.PromoterId)
                .ForeignKey("dbo.Socials", t => t.SocialId)
                .ForeignKey("dbo.Promoters", t => t.PromoterId)
                .Index(t => new { t.SocialId, t.PromoterId }, unique: true, name: "IX_SocialPromoterInvitation");
            
            CreateTable(
                "dbo.Teachers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(),
                        Resume = c.String(),
                        FacebookLink = c.String(),
                        Website = c.String(),
                        ContactEmail = c.String(),
                        Approved = c.Boolean(),
                        ApproveDate = c.DateTime(),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.Series",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Frequency = c.Int(nullable: false),
                        Day = c.Int(nullable: false),
                        Time = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Price = c.Decimal(precision: 18, scale: 2),
                        IsAvailable = c.Boolean(nullable: false),
                        Place_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Place_Id)
                .Index(t => t.Place_Id);
            
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReviewText = c.String(nullable: false),
                        Like = c.Boolean(nullable: false),
                        ReviewDate = c.DateTime(nullable: false),
                        Author_Id = c.String(maxLength: 128),
                        Event_Id = c.Int(),
                        Place_Id = c.Int(),
                        Teacher_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Author_Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .ForeignKey("dbo.Places", t => t.Place_Id)
                .ForeignKey("dbo.Teachers", t => t.Teacher_Id)
                .Index(t => t.Author_Id)
                .Index(t => t.Event_Id)
                .Index(t => t.Place_Id)
                .Index(t => t.Teacher_Id);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.Int(nullable: false),
                        DancerId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.DancerId)
                .ForeignKey("dbo.Teachers", t => t.TeacherId)
                .Index(t => new { t.TeacherId, t.DancerId }, unique: true, name: "IX_Student");
            
            CreateTable(
                "dbo.Playlists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        PublishDate = c.DateTime(nullable: false),
                        CoverPhoto = c.String(),
                        YouTubeUrl = c.String(),
                        YouTubeId = c.String(),
                        MediaSource = c.Int(nullable: false),
                        Author_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Author_Id)
                .Index(t => t.Author_Id);
            
            CreateTable(
                "dbo.Videos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        PublishDate = c.DateTime(nullable: false),
                        VideoUrl = c.String(),
                        PhotoUrl = c.String(),
                        FacebookId = c.String(),
                        YoutubeId = c.String(),
                        YoutubeUrl = c.String(),
                        YoutubeThumbnail = c.String(),
                        YouTubePlaylistUrl = c.String(),
                        YouTubePlaylistTitle = c.String(),
                        MediaSource = c.Int(nullable: false),
                        Author_Id = c.String(maxLength: 128),
                        PlayList_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Author_Id)
                .ForeignKey("dbo.Playlists", t => t.PlayList_Id)
                .Index(t => t.Author_Id)
                .Index(t => t.PlayList_Id);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .ForeignKey("dbo.Roles", t => t.RoleId)
                .Index(t => t.RoleId)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupName = c.String(),
                        GroupDescription = c.String(),
                        SkillLevel = c.Int(nullable: false),
                        FacebookLink = c.String(),
                        Public = c.Boolean(nullable: false),
                        ParentGroupID = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.DanceStyleApplicationUsers",
                c => new
                    {
                        DanceStyle_Id = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.DanceStyle_Id, t.ApplicationUser_Id })
                .ForeignKey("dbo.DanceStyles", t => t.DanceStyle_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.DanceStyle_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.EventDanceStyles",
                c => new
                    {
                        Event_Id = c.Int(nullable: false),
                        DanceStyle_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Event_Id, t.DanceStyle_Id })
                .ForeignKey("dbo.Events", t => t.Event_Id, cascadeDelete: true)
                .ForeignKey("dbo.DanceStyles", t => t.DanceStyle_Id, cascadeDelete: true)
                .Index(t => t.Event_Id)
                .Index(t => t.DanceStyle_Id);
            
            CreateTable(
                "dbo.OwnerClasses",
                c => new
                    {
                        Owner_Id = c.Int(nullable: false),
                        Class_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Owner_Id, t.Class_Id })
                .ForeignKey("dbo.Owners", t => t.Owner_Id, cascadeDelete: true)
                .ForeignKey("dbo.Classes", t => t.Class_Id, cascadeDelete: true)
                .Index(t => t.Owner_Id)
                .Index(t => t.Class_Id);
            
            CreateTable(
                "dbo.OwnerPlaces",
                c => new
                    {
                        Owner_Id = c.Int(nullable: false),
                        Place_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Owner_Id, t.Place_Id })
                .ForeignKey("dbo.Owners", t => t.Owner_Id, cascadeDelete: true)
                .ForeignKey("dbo.Places", t => t.Place_Id, cascadeDelete: true)
                .Index(t => t.Owner_Id)
                .Index(t => t.Place_Id);
            
            CreateTable(
                "dbo.SocialOwners",
                c => new
                    {
                        Social_Id = c.Int(nullable: false),
                        Owner_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Social_Id, t.Owner_Id })
                .ForeignKey("dbo.Socials", t => t.Social_Id, cascadeDelete: true)
                .ForeignKey("dbo.Owners", t => t.Owner_Id, cascadeDelete: true)
                .Index(t => t.Social_Id)
                .Index(t => t.Owner_Id);
            
            CreateTable(
                "dbo.PromoterPlaces",
                c => new
                    {
                        Promoter_Id = c.Int(nullable: false),
                        Place_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Promoter_Id, t.Place_Id })
                .ForeignKey("dbo.Promoters", t => t.Promoter_Id, cascadeDelete: true)
                .ForeignKey("dbo.Places", t => t.Place_Id, cascadeDelete: true)
                .Index(t => t.Promoter_Id)
                .Index(t => t.Place_Id);
            
            CreateTable(
                "dbo.TeacherClasses",
                c => new
                    {
                        Teacher_Id = c.Int(nullable: false),
                        Class_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Teacher_Id, t.Class_Id })
                .ForeignKey("dbo.Teachers", t => t.Teacher_Id, cascadeDelete: true)
                .ForeignKey("dbo.Classes", t => t.Class_Id, cascadeDelete: true)
                .Index(t => t.Teacher_Id)
                .Index(t => t.Class_Id);
            
            CreateTable(
                "dbo.ClassSeriesTeachers",
                c => new
                    {
                        ClassSeries_Id = c.Int(nullable: false),
                        Teacher_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ClassSeries_Id, t.Teacher_Id })
                .ForeignKey("dbo.ClassSeries", t => t.ClassSeries_Id, cascadeDelete: true)
                .ForeignKey("dbo.Teachers", t => t.Teacher_Id, cascadeDelete: true)
                .Index(t => t.ClassSeries_Id)
                .Index(t => t.Teacher_Id);
            
            CreateTable(
                "dbo.TeacherDanceStyles",
                c => new
                    {
                        Teacher_Id = c.Int(nullable: false),
                        DanceStyle_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Teacher_Id, t.DanceStyle_Id })
                .ForeignKey("dbo.Teachers", t => t.Teacher_Id, cascadeDelete: true)
                .ForeignKey("dbo.DanceStyles", t => t.DanceStyle_Id, cascadeDelete: true)
                .Index(t => t.Teacher_Id)
                .Index(t => t.DanceStyle_Id);
            
            CreateTable(
                "dbo.TeacherPlaces",
                c => new
                    {
                        Teacher_Id = c.Int(nullable: false),
                        Place_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Teacher_Id, t.Place_Id })
                .ForeignKey("dbo.Teachers", t => t.Teacher_Id, cascadeDelete: true)
                .ForeignKey("dbo.Places", t => t.Place_Id, cascadeDelete: true)
                .Index(t => t.Teacher_Id)
                .Index(t => t.Place_Id);
            
            CreateTable(
                "dbo.WorkshopTeachers",
                c => new
                    {
                        Workshop_Id = c.Int(nullable: false),
                        Teacher_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Workshop_Id, t.Teacher_Id })
                .ForeignKey("dbo.Workshops", t => t.Workshop_Id, cascadeDelete: true)
                .ForeignKey("dbo.Teachers", t => t.Teacher_Id, cascadeDelete: true)
                .Index(t => t.Workshop_Id)
                .Index(t => t.Teacher_Id);
            
            CreateTable(
                "dbo.PromoterSocials",
                c => new
                    {
                        Promoter_Id = c.Int(nullable: false),
                        Social_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Promoter_Id, t.Social_Id })
                .ForeignKey("dbo.Promoters", t => t.Promoter_Id, cascadeDelete: true)
                .ForeignKey("dbo.Socials", t => t.Social_Id, cascadeDelete: true)
                .Index(t => t.Promoter_Id)
                .Index(t => t.Social_Id);
            
            CreateTable(
                "dbo.SeriesDanceStyles",
                c => new
                    {
                        Series_Id = c.Int(nullable: false),
                        DanceStyle_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Series_Id, t.DanceStyle_Id })
                .ForeignKey("dbo.Series", t => t.Series_Id, cascadeDelete: true)
                .ForeignKey("dbo.DanceStyles", t => t.DanceStyle_Id, cascadeDelete: true)
                .Index(t => t.Series_Id)
                .Index(t => t.DanceStyle_Id);
            
            CreateTable(
                "dbo.Theaters",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Hotels",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Restaurants",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Nightclubs",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ConferenceCenters",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Studios",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.OtherPlaces",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Places", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ClassSeries",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        SkillLevel = c.Int(nullable: false),
                        Prerequisite = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Series", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Classes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        SkillLevel = c.Int(nullable: false),
                        Prerequisite = c.String(),
                        ClassType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Conferences",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Rehearsals",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Teacher_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .ForeignKey("dbo.Teachers", t => t.Teacher_Id)
                .Index(t => t.Id)
                .Index(t => t.Teacher_Id);
            
            CreateTable(
                "dbo.Workshops",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        SkillLevel = c.Int(nullable: false),
                        Prerequisite = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.OpenHouses",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Concerts",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Socials",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        SocialType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Parties",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Dancer_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .ForeignKey("dbo.Users", t => t.Dancer_Id)
                .Index(t => t.Id)
                .Index(t => t.Dancer_Id);
            
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        TeamManagerName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Schools",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.UserPictures",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        User_Id = c.String(maxLength: 128),
                        ProfilePicture = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Pictures", t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.EventPictures",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Event_Id = c.Int(),
                        PostedBy_Id = c.String(nullable: false, maxLength: 128),
                        MainPicture = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Pictures", t => t.Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .ForeignKey("dbo.Users", t => t.PostedBy_Id)
                .Index(t => t.Id)
                .Index(t => t.Event_Id)
                .Index(t => t.PostedBy_Id);
            
            CreateTable(
                "dbo.EventVideos",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Event_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Videos", t => t.Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .Index(t => t.Id)
                .Index(t => t.Event_Id);
            
            CreateTable(
                "dbo.EventPlaylists",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Event_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Playlists", t => t.Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .Index(t => t.Id)
                .Index(t => t.Event_Id);
            
            CreateTable(
                "dbo.DanceStyleVideos",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DanceStyle_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Videos", t => t.Id)
                .ForeignKey("dbo.DanceStyles", t => t.DanceStyle_Id)
                .Index(t => t.Id)
                .Index(t => t.DanceStyle_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DanceStyleVideos", "DanceStyle_Id", "dbo.DanceStyles");
            DropForeignKey("dbo.DanceStyleVideos", "Id", "dbo.Videos");
            DropForeignKey("dbo.EventPlaylists", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.EventPlaylists", "Id", "dbo.Playlists");
            DropForeignKey("dbo.EventVideos", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.EventVideos", "Id", "dbo.Videos");
            DropForeignKey("dbo.EventPictures", "PostedBy_Id", "dbo.Users");
            DropForeignKey("dbo.EventPictures", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.EventPictures", "Id", "dbo.Pictures");
            DropForeignKey("dbo.UserPictures", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserPictures", "Id", "dbo.Pictures");
            DropForeignKey("dbo.Schools", "Id", "dbo.Groups");
            DropForeignKey("dbo.Teams", "Id", "dbo.Groups");
            DropForeignKey("dbo.Parties", "Dancer_Id", "dbo.Users");
            DropForeignKey("dbo.Parties", "Id", "dbo.Events");
            DropForeignKey("dbo.Socials", "Id", "dbo.Events");
            DropForeignKey("dbo.Concerts", "Id", "dbo.Events");
            DropForeignKey("dbo.OpenHouses", "Id", "dbo.Events");
            DropForeignKey("dbo.Workshops", "Id", "dbo.Events");
            DropForeignKey("dbo.Rehearsals", "Teacher_Id", "dbo.Teachers");
            DropForeignKey("dbo.Rehearsals", "Id", "dbo.Events");
            DropForeignKey("dbo.Conferences", "Id", "dbo.Events");
            DropForeignKey("dbo.Classes", "Id", "dbo.Events");
            DropForeignKey("dbo.ClassSeries", "Id", "dbo.Series");
            DropForeignKey("dbo.OtherPlaces", "Id", "dbo.Places");
            DropForeignKey("dbo.Studios", "Id", "dbo.Places");
            DropForeignKey("dbo.ConferenceCenters", "Id", "dbo.Places");
            DropForeignKey("dbo.Nightclubs", "Id", "dbo.Places");
            DropForeignKey("dbo.Restaurants", "Id", "dbo.Places");
            DropForeignKey("dbo.Hotels", "Id", "dbo.Places");
            DropForeignKey("dbo.Theaters", "Id", "dbo.Places");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.UserRoles", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.UserLogins", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.UserClaims", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.Events", "ApplicationUser_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.Series", "Place_Id", "dbo.Places");
            DropForeignKey("dbo.Events", "Series_Id", "dbo.Series");
            DropForeignKey("dbo.SeriesDanceStyles", "DanceStyle_Id", "dbo.DanceStyles");
            DropForeignKey("dbo.SeriesDanceStyles", "Series_Id", "dbo.Series");
            DropForeignKey("dbo.Videos", "PlayList_Id", "dbo.Playlists");
            DropForeignKey("dbo.Playlists", "Author_Id", "dbo.Users");
            DropForeignKey("dbo.Videos", "Author_Id", "dbo.Users");
            DropForeignKey("dbo.PromoterSocials", "Social_Id", "dbo.Socials");
            DropForeignKey("dbo.PromoterSocials", "Promoter_Id", "dbo.Promoters");
            DropForeignKey("dbo.SocialPromoterInvitations", "PromoterId", "dbo.Promoters");
            DropForeignKey("dbo.SocialPromoterInvitations", "SocialId", "dbo.Socials");
            DropForeignKey("dbo.SocialPromoterInvitations", "PromoterId", "dbo.Teachers");
            DropForeignKey("dbo.WorkshopTeachers", "Teacher_Id", "dbo.Teachers");
            DropForeignKey("dbo.WorkshopTeachers", "Workshop_Id", "dbo.Workshops");
            DropForeignKey("dbo.Students", "TeacherId", "dbo.Teachers");
            DropForeignKey("dbo.Students", "DancerId", "dbo.Users");
            DropForeignKey("dbo.Reviews", "Teacher_Id", "dbo.Teachers");
            DropForeignKey("dbo.Reviews", "Place_Id", "dbo.Places");
            DropForeignKey("dbo.Reviews", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.Reviews", "Author_Id", "dbo.Users");
            DropForeignKey("dbo.TeacherPlaces", "Place_Id", "dbo.Places");
            DropForeignKey("dbo.TeacherPlaces", "Teacher_Id", "dbo.Teachers");
            DropForeignKey("dbo.TeacherDanceStyles", "DanceStyle_Id", "dbo.DanceStyles");
            DropForeignKey("dbo.TeacherDanceStyles", "Teacher_Id", "dbo.Teachers");
            DropForeignKey("dbo.ClassTeacherInvitations", "TeacherId", "dbo.Teachers");
            DropForeignKey("dbo.ClassSeriesTeachers", "Teacher_Id", "dbo.Teachers");
            DropForeignKey("dbo.ClassSeriesTeachers", "ClassSeries_Id", "dbo.ClassSeries");
            DropForeignKey("dbo.TeacherClasses", "Class_Id", "dbo.Classes");
            DropForeignKey("dbo.TeacherClasses", "Teacher_Id", "dbo.Teachers");
            DropForeignKey("dbo.Teachers", "ApplicationUser_Id", "dbo.Users");
            DropForeignKey("dbo.PromoterPlaces", "Place_Id", "dbo.Places");
            DropForeignKey("dbo.PromoterPlaces", "Promoter_Id", "dbo.Promoters");
            DropForeignKey("dbo.Promoters", "ApplicationUser_Id", "dbo.Users");
            DropForeignKey("dbo.SocialOwners", "Owner_Id", "dbo.Owners");
            DropForeignKey("dbo.SocialOwners", "Social_Id", "dbo.Socials");
            DropForeignKey("dbo.OwnerPlaces", "Place_Id", "dbo.Places");
            DropForeignKey("dbo.OwnerPlaces", "Owner_Id", "dbo.Owners");
            DropForeignKey("dbo.PlaceOwnerInvitations", "PlaceId", "dbo.Places");
            DropForeignKey("dbo.PlaceOwnerInvitations", "OwnerId", "dbo.Owners");
            DropForeignKey("dbo.OwnerClasses", "Class_Id", "dbo.Classes");
            DropForeignKey("dbo.OwnerClasses", "Owner_Id", "dbo.Owners");
            DropForeignKey("dbo.Owners", "ApplicationUser_Id", "dbo.Users");
            DropForeignKey("dbo.Events", "Place_Id", "dbo.Places");
            DropForeignKey("dbo.LinkedFacebookObjects", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.EventMembers", "UserId", "dbo.Users");
            DropForeignKey("dbo.EventMembers", "EventId", "dbo.Events");
            DropForeignKey("dbo.EventDanceStyles", "DanceStyle_Id", "dbo.DanceStyles");
            DropForeignKey("dbo.EventDanceStyles", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.Events", "Creator_Id", "dbo.Users");
            DropForeignKey("dbo.DanceStyleApplicationUsers", "ApplicationUser_Id", "dbo.Users");
            DropForeignKey("dbo.DanceStyleApplicationUsers", "DanceStyle_Id", "dbo.DanceStyles");
            DropForeignKey("dbo.ClassTeacherInvitations", "ClassId", "dbo.Classes");
            DropIndex("dbo.DanceStyleVideos", new[] { "DanceStyle_Id" });
            DropIndex("dbo.DanceStyleVideos", new[] { "Id" });
            DropIndex("dbo.EventPlaylists", new[] { "Event_Id" });
            DropIndex("dbo.EventPlaylists", new[] { "Id" });
            DropIndex("dbo.EventVideos", new[] { "Event_Id" });
            DropIndex("dbo.EventVideos", new[] { "Id" });
            DropIndex("dbo.EventPictures", new[] { "PostedBy_Id" });
            DropIndex("dbo.EventPictures", new[] { "Event_Id" });
            DropIndex("dbo.EventPictures", new[] { "Id" });
            DropIndex("dbo.UserPictures", new[] { "User_Id" });
            DropIndex("dbo.UserPictures", new[] { "Id" });
            DropIndex("dbo.Schools", new[] { "Id" });
            DropIndex("dbo.Teams", new[] { "Id" });
            DropIndex("dbo.Parties", new[] { "Dancer_Id" });
            DropIndex("dbo.Parties", new[] { "Id" });
            DropIndex("dbo.Socials", new[] { "Id" });
            DropIndex("dbo.Concerts", new[] { "Id" });
            DropIndex("dbo.OpenHouses", new[] { "Id" });
            DropIndex("dbo.Workshops", new[] { "Id" });
            DropIndex("dbo.Rehearsals", new[] { "Teacher_Id" });
            DropIndex("dbo.Rehearsals", new[] { "Id" });
            DropIndex("dbo.Conferences", new[] { "Id" });
            DropIndex("dbo.Classes", new[] { "Id" });
            DropIndex("dbo.ClassSeries", new[] { "Id" });
            DropIndex("dbo.OtherPlaces", new[] { "Id" });
            DropIndex("dbo.Studios", new[] { "Id" });
            DropIndex("dbo.ConferenceCenters", new[] { "Id" });
            DropIndex("dbo.Nightclubs", new[] { "Id" });
            DropIndex("dbo.Restaurants", new[] { "Id" });
            DropIndex("dbo.Hotels", new[] { "Id" });
            DropIndex("dbo.Theaters", new[] { "Id" });
            DropIndex("dbo.SeriesDanceStyles", new[] { "DanceStyle_Id" });
            DropIndex("dbo.SeriesDanceStyles", new[] { "Series_Id" });
            DropIndex("dbo.PromoterSocials", new[] { "Social_Id" });
            DropIndex("dbo.PromoterSocials", new[] { "Promoter_Id" });
            DropIndex("dbo.WorkshopTeachers", new[] { "Teacher_Id" });
            DropIndex("dbo.WorkshopTeachers", new[] { "Workshop_Id" });
            DropIndex("dbo.TeacherPlaces", new[] { "Place_Id" });
            DropIndex("dbo.TeacherPlaces", new[] { "Teacher_Id" });
            DropIndex("dbo.TeacherDanceStyles", new[] { "DanceStyle_Id" });
            DropIndex("dbo.TeacherDanceStyles", new[] { "Teacher_Id" });
            DropIndex("dbo.ClassSeriesTeachers", new[] { "Teacher_Id" });
            DropIndex("dbo.ClassSeriesTeachers", new[] { "ClassSeries_Id" });
            DropIndex("dbo.TeacherClasses", new[] { "Class_Id" });
            DropIndex("dbo.TeacherClasses", new[] { "Teacher_Id" });
            DropIndex("dbo.PromoterPlaces", new[] { "Place_Id" });
            DropIndex("dbo.PromoterPlaces", new[] { "Promoter_Id" });
            DropIndex("dbo.SocialOwners", new[] { "Owner_Id" });
            DropIndex("dbo.SocialOwners", new[] { "Social_Id" });
            DropIndex("dbo.OwnerPlaces", new[] { "Place_Id" });
            DropIndex("dbo.OwnerPlaces", new[] { "Owner_Id" });
            DropIndex("dbo.OwnerClasses", new[] { "Class_Id" });
            DropIndex("dbo.OwnerClasses", new[] { "Owner_Id" });
            DropIndex("dbo.EventDanceStyles", new[] { "DanceStyle_Id" });
            DropIndex("dbo.EventDanceStyles", new[] { "Event_Id" });
            DropIndex("dbo.DanceStyleApplicationUsers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.DanceStyleApplicationUsers", new[] { "DanceStyle_Id" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.UserRoles", new[] { "IdentityUser_Id" });
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserLogins", new[] { "IdentityUser_Id" });
            DropIndex("dbo.Videos", new[] { "PlayList_Id" });
            DropIndex("dbo.Videos", new[] { "Author_Id" });
            DropIndex("dbo.Playlists", new[] { "Author_Id" });
            DropIndex("dbo.Students", "IX_Student");
            DropIndex("dbo.Reviews", new[] { "Teacher_Id" });
            DropIndex("dbo.Reviews", new[] { "Place_Id" });
            DropIndex("dbo.Reviews", new[] { "Event_Id" });
            DropIndex("dbo.Reviews", new[] { "Author_Id" });
            DropIndex("dbo.Series", new[] { "Place_Id" });
            DropIndex("dbo.Teachers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.SocialPromoterInvitations", "IX_SocialPromoterInvitation");
            DropIndex("dbo.Promoters", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.PlaceOwnerInvitations", "IX_PlaceOwnerInvitation");
            DropIndex("dbo.Owners", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.LinkedFacebookObjects", new[] { "Event_Id" });
            DropIndex("dbo.EventMembers", "IX_EventMembers");
            DropIndex("dbo.DanceStyles", "NameIndex");
            DropIndex("dbo.UserClaims", new[] { "IdentityUser_Id" });
            DropIndex("dbo.Users", new[] { "Event_Id" });
            DropIndex("dbo.Events", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.Events", new[] { "Series_Id" });
            DropIndex("dbo.Events", new[] { "Place_Id" });
            DropIndex("dbo.Events", new[] { "Creator_Id" });
            DropIndex("dbo.ClassTeacherInvitations", "IX_ClassTeacherInvitation");
            DropTable("dbo.DanceStyleVideos");
            DropTable("dbo.EventPlaylists");
            DropTable("dbo.EventVideos");
            DropTable("dbo.EventPictures");
            DropTable("dbo.UserPictures");
            DropTable("dbo.Schools");
            DropTable("dbo.Teams");
            DropTable("dbo.Parties");
            DropTable("dbo.Socials");
            DropTable("dbo.Concerts");
            DropTable("dbo.OpenHouses");
            DropTable("dbo.Workshops");
            DropTable("dbo.Rehearsals");
            DropTable("dbo.Conferences");
            DropTable("dbo.Classes");
            DropTable("dbo.ClassSeries");
            DropTable("dbo.OtherPlaces");
            DropTable("dbo.Studios");
            DropTable("dbo.ConferenceCenters");
            DropTable("dbo.Nightclubs");
            DropTable("dbo.Restaurants");
            DropTable("dbo.Hotels");
            DropTable("dbo.Theaters");
            DropTable("dbo.SeriesDanceStyles");
            DropTable("dbo.PromoterSocials");
            DropTable("dbo.WorkshopTeachers");
            DropTable("dbo.TeacherPlaces");
            DropTable("dbo.TeacherDanceStyles");
            DropTable("dbo.ClassSeriesTeachers");
            DropTable("dbo.TeacherClasses");
            DropTable("dbo.PromoterPlaces");
            DropTable("dbo.SocialOwners");
            DropTable("dbo.OwnerPlaces");
            DropTable("dbo.OwnerClasses");
            DropTable("dbo.EventDanceStyles");
            DropTable("dbo.DanceStyleApplicationUsers");
            DropTable("dbo.Roles");
            DropTable("dbo.Groups");
            DropTable("dbo.UserRoles");
            DropTable("dbo.UserLogins");
            DropTable("dbo.Videos");
            DropTable("dbo.Playlists");
            DropTable("dbo.Students");
            DropTable("dbo.Reviews");
            DropTable("dbo.Series");
            DropTable("dbo.Teachers");
            DropTable("dbo.SocialPromoterInvitations");
            DropTable("dbo.Promoters");
            DropTable("dbo.PlaceOwnerInvitations");
            DropTable("dbo.Owners");
            DropTable("dbo.Places");
            DropTable("dbo.Pictures");
            DropTable("dbo.LinkedFacebookObjects");
            DropTable("dbo.EventMembers");
            DropTable("dbo.DanceStyles");
            DropTable("dbo.UserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.Events");
            DropTable("dbo.ClassTeacherInvitations");
        }
    }
}
