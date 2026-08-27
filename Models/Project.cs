using ProjectManagementSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Project
    {
        public Project()
        {
            Members = new HashSet<ProjectMember>();
            Tasks = new HashSet<TaskItem>();
        }

        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ProjectStatus Status { get; set; }

        public int ProgressPercentage { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        public string ProjectManagerId { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsArchived { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("ProjectManagerId")]
        public virtual ApplicationUser ProjectManager { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser CreatedByUser { get; set; }

        public virtual ICollection<ProjectMember> Members { get; set; }

        public virtual ICollection<TaskItem> Tasks { get; set; }
    }
}