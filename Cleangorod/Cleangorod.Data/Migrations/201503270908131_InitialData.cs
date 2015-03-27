namespace Cleangorod.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CollectionDateTimeRanges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WeekStart = c.DateTime(nullable: false),
                        Date = c.DateTime(nullable: false),
                        StartHour = c.Int(nullable: false),
                        EndHour = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ClientSchedules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Note = c.String(),
                        WeekStart = c.DateTime(nullable: false),
                        Client_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Client_Id)
                .Index(t => t.Client_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Surname = c.String(),
                        Note = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                        ClientAddress_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ClientAddresses", t => t.ClientAddress_Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.ClientAddress_Id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ClientAddresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Address = c.String(),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.ClientScheduleCollectionDateTimeRanges",
                c => new
                    {
                        ClientSchedule_Id = c.Int(nullable: false),
                        CollectionDateTimeRange_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ClientSchedule_Id, t.CollectionDateTimeRange_Id })
                .ForeignKey("dbo.ClientSchedules", t => t.ClientSchedule_Id, cascadeDelete: true)
                .ForeignKey("dbo.CollectionDateTimeRanges", t => t.CollectionDateTimeRange_Id, cascadeDelete: true)
                .Index(t => t.ClientSchedule_Id)
                .Index(t => t.CollectionDateTimeRange_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.ClientScheduleCollectionDateTimeRanges", "CollectionDateTimeRange_Id", "dbo.CollectionDateTimeRanges");
            DropForeignKey("dbo.ClientScheduleCollectionDateTimeRanges", "ClientSchedule_Id", "dbo.ClientSchedules");
            DropForeignKey("dbo.ClientSchedules", "Client_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "ClientAddress_Id", "dbo.ClientAddresses");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.ClientScheduleCollectionDateTimeRanges", new[] { "CollectionDateTimeRange_Id" });
            DropIndex("dbo.ClientScheduleCollectionDateTimeRanges", new[] { "ClientSchedule_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", new[] { "ClientAddress_Id" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.ClientSchedules", new[] { "Client_Id" });
            DropTable("dbo.ClientScheduleCollectionDateTimeRanges");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.ClientAddresses");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.ClientSchedules");
            DropTable("dbo.CollectionDateTimeRanges");
        }
    }
}
