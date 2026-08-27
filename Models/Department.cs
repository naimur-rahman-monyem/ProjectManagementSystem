using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class Department
    {
        public Department()
        {
            Users = new HashSet<ApplicationUser>();
            Projects = new HashSet<Project>();
        }

        public int DepartmentId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}