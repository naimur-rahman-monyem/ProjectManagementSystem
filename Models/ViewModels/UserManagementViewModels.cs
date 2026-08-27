using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "System Role")]
        public string RoleName { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
    }

    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "System Role")]
        public string RoleName { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Active Account")]
        public bool IsActive { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password (leave blank to keep current)")]
        public string NewPassword { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
    }
}
