using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ProjectManagementSystem.Models.Enums;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class ProjectFormViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Target End Date")]
        public DateTime? EndDate { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        [Range(0, 100)]
        [Display(Name = "Progress (%)")]
        public int ProgressPercentage { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Required]
        [Display(Name = "Project Manager")]
        public string ProjectManagerId { get; set; }

        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> ProjectManagerList { get; set; }
    }

    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public List<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public bool CanEditProject { get; set; }
        public bool CanManageMembers { get; set; }
        public bool CanCreateTask { get; set; }
    }

    public class ProjectMemberViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<ProjectMember> CurrentMembers { get; set; } = new List<ProjectMember>();

        [Required]
        [Display(Name = "Select Team Member")]
        public string SelectedUserId { get; set; }

        public IEnumerable<SelectListItem> AvailableUsersList { get; set; }
    }
}
