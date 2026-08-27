using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class DepartmentFormViewModel
    {
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Department Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Active Department")]
        public bool IsActive { get; set; } = true;
    }

    public class DepartmentDetailsViewModel
    {
        public Department Department { get; set; }
        public List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
        public List<Project> Projects { get; set; } = new List<Project>();
    }
}
