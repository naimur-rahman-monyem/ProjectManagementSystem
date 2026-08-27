using System.Collections.Generic;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        public List<SystemSetting> Settings { get; set; } = new List<SystemSetting>();
        public List<EmailLog> RecentEmailLogs { get; set; } = new List<EmailLog>();
        public List<AuditLog> RecentAuditLogs { get; set; } = new List<AuditLog>();
    }
}
