using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.Enums;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController()
        {
            _db = new ApplicationDbContext();
        }

        // GET: /Dashboard
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = _db.Users.FirstOrDefault(u => u.Id == currentUserId);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var model = new DashboardViewModel
            {
                CurrentUserId = currentUserId,
                CurrentUserName = $"{currentUser.FirstName} {currentUser.LastName}",
            };

            if (User.IsInRole("Admin"))
            {
                model.UserRole = "Admin";
                model.TotalProjects = _db.Projects.Count();
                model.ActiveProjects = _db.Projects.Count(p => p.Status == ProjectStatus.Active);
                model.CompletedProjects = _db.Projects.Count(p => p.Status == ProjectStatus.Completed);

                model.TotalTasks = _db.TaskItems.Count();
                model.PendingTasks = _db.TaskItems.Count(t => t.Status == TaskStatus.Pending);
                model.InProgressTasks = _db.TaskItems.Count(t => t.Status == TaskStatus.InProgress);
                model.CompletedTasks = _db.TaskItems.Count(t => t.Status == TaskStatus.Completed);
                model.OverdueTasks = _db.TaskItems.Count(t => t.DueDate < DateTime.Now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);

                model.TotalUsers = _db.Users.Count();
                model.ActiveUsers = _db.Users.Count(u => u.IsActive);
                model.TotalDepartments = _db.Departments.Count(d => d.IsActive);

                model.RecentProjects = _db.Projects.Include(p => p.ProjectManager).Include(p => p.Department)
                                            .OrderByDescending(p => p.CreatedDate).Take(5).ToList();
                model.OverdueTaskList = _db.TaskItems.Include(t => t.Project).Include(t => t.AssignedToUser)
                                            .Where(t => t.DueDate < DateTime.Now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled)
                                            .OrderBy(t => t.DueDate).Take(5).ToList();
            }
            else if (User.IsInRole("ProjectManager"))
            {
                model.UserRole = "ProjectManager";
                var managedProjects = _db.Projects.Where(p => p.ProjectManagerId == currentUserId);
                model.TotalProjects = managedProjects.Count();
                model.ActiveProjects = managedProjects.Count(p => p.Status == ProjectStatus.Active);
                model.CompletedProjects = managedProjects.Count(p => p.Status == ProjectStatus.Completed);

                var managedTaskItems = _db.TaskItems.Where(t => t.Project.ProjectManagerId == currentUserId);
                model.TotalTasks = managedTaskItems.Count();
                model.PendingTasks = managedTaskItems.Count(t => t.Status == TaskStatus.Pending);
                model.InProgressTasks = managedTaskItems.Count(t => t.Status == TaskStatus.InProgress);
                model.CompletedTasks = managedTaskItems.Count(t => t.Status == TaskStatus.Completed);
                model.OverdueTasks = managedTaskItems.Count(t => t.DueDate < DateTime.Now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);

                model.RecentProjects = managedProjects.Include(p => p.Department).OrderByDescending(p => p.CreatedDate).Take(5).ToList();
                model.OverdueTaskList = managedTaskItems.Include(t => t.Project).Include(t => t.AssignedToUser)
                                                .Where(t => t.DueDate < DateTime.Now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled)
                                                .OrderBy(t => t.DueDate).Take(5).ToList();
                model.AssignedTasks = managedTaskItems.Include(t => t.Project).Include(t => t.AssignedToUser)
                                              .OrderByDescending(t => t.CreatedDate).Take(5).ToList();
            }
            else // Normal User
            {
                model.UserRole = "User";
                var userMemberProjectIds = _db.ProjectMembers.Where(pm => pm.UserId == currentUserId && pm.IsActive).Select(pm => pm.ProjectId).ToList();
                var userProjects = _db.Projects.Where(p => userMemberProjectIds.Contains(p.ProjectId));
                
                model.TotalProjects = userProjects.Count();
                model.ActiveProjects = userProjects.Count(p => p.Status == ProjectStatus.Active);
                model.CompletedProjects = userProjects.Count(p => p.Status == ProjectStatus.Completed);

                var userAssignedTasks = _db.TaskItems.Where(t => t.AssignedToUserId == currentUserId);
                model.TotalTasks = userAssignedTasks.Count();
                model.PendingTasks = userAssignedTasks.Count(t => t.Status == TaskStatus.Pending);
                model.InProgressTasks = userAssignedTasks.Count(t => t.Status == TaskStatus.InProgress);
                model.CompletedTasks = userAssignedTasks.Count(t => t.Status == TaskStatus.Completed);
                model.OverdueTasks = userAssignedTasks.Count(t => t.DueDate < DateTime.Now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);

                model.RecentProjects = userProjects.Include(p => p.ProjectManager).Take(5).ToList();
                model.AssignedTasks = userAssignedTasks.Include(t => t.Project).OrderBy(t => t.DueDate).Take(10).ToList();
                model.OverdueTaskList = userAssignedTasks.Include(t => t.Project)
                                                .Where(t => t.DueDate < DateTime.Now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled)
                                                .OrderBy(t => t.DueDate).ToList();
            }

            model.UserNotifications = _db.Notifications.Where(n => n.UserId == currentUserId && !n.IsRead)
                                           .OrderByDescending(n => n.CreatedDate).Take(5).ToList();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
