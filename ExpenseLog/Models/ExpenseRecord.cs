using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ExpenseLog.Models
{
    public class ExpenseRecord
    {
        public int ExpenseRecordID { get; set; }

        [Required]
        [Display(Name = "Type")]
        [Range(typeof(int), "1", "9999999", ErrorMessage = "Select expense type.")]
        public int ExpenseTypeID { get; set; }

        [Required]
        [Display(Name = "Entity")]
        [Range(typeof(int), "1", "9999999", ErrorMessage ="Select expense entity.")]
        public int ExpenseEntityID { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Column(TypeName = "Date")]
        public DateTime ExpenseDate { get; set; }

        [Required]
        [Display(Name = "Price")]
        [Range(typeof(decimal), "0", "99999")]
        public decimal ExpensePrice { get; set; }
                
        [Display(Name = "Description")]
        [MaxLength(256)]
        public string ExpenseDescription { get; set; }
        
        [MaxLength(50)]
        public string UserId { get; set; }

        [Display(Name = "LogDate")]
        public DateTime ExpenseLogDate { get; set; }

        [Display(Name = "Type")]
        public virtual ExpenseType ExpenseType { get; set; }

        [Display(Name = "Entity")]
        public virtual ExpenseEntity ExpenseEntity { get; set; }

        [Display(Name = "Attachments")]
        //navigation property ExpenseAttachments
        public virtual ICollection<ExpenseAttachment> ExpenseAttachments { get; set; }
    }
}