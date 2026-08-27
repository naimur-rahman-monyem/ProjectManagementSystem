using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class EmailLog
    {
        public int EmailLogId { get; set; }

        [Required]
        [StringLength(256)]
        public string RecipientEmail { get; set; }

        [Required]
        [StringLength(500)]
        public string Subject { get; set; }

        public string Body { get; set; }

        [StringLength(100)]
        public string EmailType { get; set; }

        public string RelatedUserId { get; set; }

        public int? RelatedProjectId { get; set; }

        public int? RelatedTaskItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime? SentDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}