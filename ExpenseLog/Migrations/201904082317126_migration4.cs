namespace ExpenseLog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExpenseAttachment", "ExpenseAttachmentType", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExpenseAttachment", "ExpenseAttachmentType");
        }
    }
}
