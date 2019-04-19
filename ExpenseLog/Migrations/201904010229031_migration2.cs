namespace ExpenseLog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExpenseAttachment", "ExpenseAttachmentUri", c => c.String(nullable: false, maxLength: 800));
            AddColumn("dbo.ExpenseAttachment", "ExpenseAttachmentLength", c => c.Int(nullable: false));
            AddColumn("dbo.ExpenseAttachment", "ExpsenseAttachmentDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.ExpenseAttachment", "ExpenseAttachmentSize");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExpenseAttachment", "ExpenseAttachmentSize", c => c.Int(nullable: false));
            DropColumn("dbo.ExpenseAttachment", "ExpsenseAttachmentDate");
            DropColumn("dbo.ExpenseAttachment", "ExpenseAttachmentLength");
            DropColumn("dbo.ExpenseAttachment", "ExpenseAttachmentUri");
        }
    }
}
