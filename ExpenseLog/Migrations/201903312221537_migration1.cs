namespace ExpenseLog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExpenseAttachment",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ExpenseRecordID = c.Int(nullable: false),
                        ExpenseAttachmentName = c.String(nullable: false, maxLength: 200),
                        ExpenseAttachmentOriginalName = c.String(nullable: false, maxLength: 200),
                        ExpenseAttachmentSize = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ExpenseRecord", t => t.ExpenseRecordID, cascadeDelete: true)
                .Index(t => t.ExpenseRecordID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExpenseAttachment", "ExpenseRecordID", "dbo.ExpenseRecord");
            DropIndex("dbo.ExpenseAttachment", new[] { "ExpenseRecordID" });
            DropTable("dbo.ExpenseAttachment");
        }
    }
}
