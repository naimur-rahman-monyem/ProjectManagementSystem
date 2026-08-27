using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class TaskComment
    {
        public int TaskCommentId { get; set; }

        public int TaskItemId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(4000)]
        public string CommentText { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("TaskItemId")]
        public virtual TaskItem TaskItem { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}