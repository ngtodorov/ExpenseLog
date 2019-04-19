namespace ExpenseLog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ExpenseAttachment", "ExpenseAttachmentLength", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ExpenseAttachment", "ExpenseAttachmentLength", c => c.Int(nullable: false));
        }
    }
}
