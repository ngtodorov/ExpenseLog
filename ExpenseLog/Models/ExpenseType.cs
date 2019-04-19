using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ExpenseLog.DAL;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace ExpenseLog.Models
{
    public class ExpenseType 
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Expense Type")]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(50)]
        public string UserId { get; set; }


        public virtual ICollection<ExpenseEntity> ExpenseEntities { get; set; }

        public virtual ICollection<ExpenseRecord> ExpenseRecords { get; set; }

 
    }
}