using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseLog.Models
{
    public class ExpenseAttachment
    {
        public int ID { get; set; }

        [Required]
        public int ExpenseRecordID { get; set; }

        [Required]
        [Display(Name = "Attachment Name")]
        [MaxLength(200)]
        public string ExpenseAttachmentName { get; set; }

        [Display(Name = "Attachment Type")]
        [MaxLength(50)]
        public string ExpenseAttachmentType { get; set; }

        [Required]
        [Display(Name = "Attachment Uri")]
        [MaxLength(800)]
        public string ExpenseAttachmentUri { get; set; }

        [Required]
        [Display(Name = "Attachment Original Name")]
        [MaxLength(200)]
        public string ExpenseAttachmentOriginalName { get; set; }
        
        public long ExpenseAttachmentLength { get; set; }

        public DateTime ExpsenseAttachmentDate { get; set; }

        [Display(Name = "Expense Record")]
        public virtual ExpenseRecord ExpenseRecord { get; set; }

    }
}