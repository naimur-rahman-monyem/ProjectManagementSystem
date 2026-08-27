using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}