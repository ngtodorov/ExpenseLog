namespace ExpenseLog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExpenseEntity",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ExpenseEntityName = c.String(nullable: false, maxLength: 200),
                        ExpenseEntityDescription = c.String(nullable: false, maxLength: 200),
                        UserId = c.String(maxLength: 50),
                        ExpenseTypeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ExpenseType", t => t.ExpenseTypeID, cascadeDelete: false)
                .Index(t => t.ExpenseTypeID);
            
            CreateTable(
                "dbo.ExpenseRecord",
                c => new
                    {
                        ExpenseRecordID = c.Int(nullable: false, identity: true),
                        ExpenseTypeID = c.Int(nullable: false),
                        ExpenseEntityID = c.Int(nullable: false),
                        ExpenseDate = c.DateTime(nullable: false, storeType: "date"),
                        ExpensePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExpenseDescription = c.String(maxLength: 256),
                        UserId = c.String(maxLength: 50),
                        ExpenseLogDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ExpenseRecordID)
                .ForeignKey("dbo.ExpenseEntity", t => t.ExpenseEntityID, cascadeDelete: false)
                .ForeignKey("dbo.ExpenseType", t => t.ExpenseTypeID, cascadeDelete: false)
                .Index(t => t.ExpenseTypeID)
                .Index(t => t.ExpenseEntityID);
            
            CreateTable(
                "dbo.ExpenseType",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        UserId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.ID);
        }
        
        public override void Down()
        {
        }
    }
}
