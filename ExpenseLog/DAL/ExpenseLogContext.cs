using System.Collections.Generic;
using ExpenseLog.Models;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.ModelConfiguration.Conventions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Infrastructure;

namespace ExpenseLog.DAL
{
    //public class ExpenseLogContext : DbContext
    public class ExpenseLogContext : IdentityDbContext<ApplicationUser>
    {

        public ExpenseLogContext() : base("ExpenseLogContext", throwIfV1Schema: false)
        {
        }

        public DbSet<ExpenseEntity> ExpenseEntities { get; set; }
        public DbSet<ExpenseRecord> ExpenseRecords { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<ExpenseAttachment> ExpenseAttachments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.UserId, r.RoleId }).ToTable("ApplicationUserRoles");

            modelBuilder.Entity<IdentityUserLogin>().HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId }).ToTable("ApplicationUserLogins");

            // the all important base class call! Add this line to make your problems go away.
            base.OnModelCreating(modelBuilder);
        }

        public static ExpenseLogContext Create()
        {
            return new ExpenseLogContext();
        }

       
    }


}

