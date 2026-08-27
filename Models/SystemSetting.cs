using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class SystemSetting
    {
        public int SystemSettingId { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; }

        public string SettingValue { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string UpdatedByUserId { get; set; }
    }
}