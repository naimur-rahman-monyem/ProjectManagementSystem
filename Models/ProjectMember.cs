using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class ProjectMember
    {
        public int ProjectMemberId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime JoinedDate { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}