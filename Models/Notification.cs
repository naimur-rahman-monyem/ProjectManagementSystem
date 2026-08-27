using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; }

        public int? ProjectId { get; set; }

        public int? TaskItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string NotificationType { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ReadDate { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("TaskItemId")]
        public virtual TaskItem TaskItem { get; set; }
    }
}