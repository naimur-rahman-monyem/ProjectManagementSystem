using ProjectManagementSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class TaskItem
    {
        public TaskItem()
        {
            Comments = new HashSet<TaskComment>();
            Files = new HashSet<TaskFile>();
            Notifications = new HashSet<Notification>();
        }

        public int TaskItemId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        [Required]
        public string AssignedToUserId { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        [Range(0, 100)]
        public int ProgressPercentage { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual ApplicationUser AssignedToUser { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser CreatedByUser { get; set; }

        public virtual ICollection<TaskComment> Comments { get; set; }

        public virtual ICollection<TaskFile> Files { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}