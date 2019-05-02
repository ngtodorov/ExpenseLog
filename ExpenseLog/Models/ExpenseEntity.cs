using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseLog.Models
{
    public class ExpenseEntity
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Entity Name")]
        [MaxLength(200)]
        public string ExpenseEntityName { get; set; }

        [Required]
        [Display(Name = "Entity Description")]
        [MaxLength(200)]
        public string ExpenseEntityDescription { get; set; }

        [MaxLength(50)]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Type")]
        [Range(typeof(int), "1", "9999999", ErrorMessage = "Expensty Type is required")]
        public int ExpenseTypeID { get; set; }

        [Display(Name = "Type")]
        public virtual ExpenseType ExpenseType { get; set; }

        //navigation property ExpenseRecords
        public virtual ICollection<ExpenseRecord> ExpenseRecords { get; set; }
    }
}