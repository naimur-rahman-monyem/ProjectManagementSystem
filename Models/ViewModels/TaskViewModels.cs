using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ProjectManagementSystem.Models.Enums;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class TaskFormViewModel
    {
        public int TaskItemId { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [StringLength(4000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Assign To")]
        public string AssignedToUserId { get; set; }

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [Range(0, 100)]
        [Display(Name = "Progress (%)")]
        public int ProgressPercentage { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date / Deadline")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        public IEnumerable<SelectListItem> ProjectList { get; set; }
        public IEnumerable<SelectListItem> AssigneeList { get; set; }
    }

    public class TaskDetailsViewModel
    {
        public TaskItem Task { get; set; }
        public List<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public List<TaskFile> Files { get; set; } = new List<TaskFile>();
        public bool CanEditTask { get; set; }
        public bool CanUpdateStatus { get; set; }
        public bool IsOverdue => Task.DueDate < DateTime.Now && Task.Status != TaskStatus.Completed && Task.Status != TaskStatus.Cancelled;
    }

    public class TaskStatusUpdateViewModel
    {
        public int TaskItemId { get; set; }
        public string TaskTitle { get; set; }

        [Required]
        public TaskStatus Status { get; set; }

        [Range(0, 100)]
        [Display(Name = "Progress (%)")]
        public int ProgressPercentage { get; set; }

        [StringLength(1000)]
        [Display(Name = "Status Update Comment (Optional)")]
        public string Comment { get; set; }
    }
}
