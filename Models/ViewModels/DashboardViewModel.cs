using System.Collections.Generic;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string CurrentUserId { get; set; }
        public string CurrentUserName { get; set; }
        public string UserRole { get; set; }

        // Stat Counters
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }

        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }

        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalDepartments { get; set; }

        // Role-Specific Lists
        public List<Project> RecentProjects { get; set; } = new List<Project>();
        public List<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public List<TaskItem> OverdueTaskList { get; set; } = new List<TaskItem>();
        public List<Notification> UserNotifications { get; set; } = new List<Notification>();
    }
}
