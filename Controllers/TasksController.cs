using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.Enums;
using ProjectManagementSystem.Models.ViewModels;
using TaskStatus = ProjectManagementSystem.Models.Enums.TaskStatus;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TasksController()
        {
            _db = new ApplicationDbContext();
        }

        // GET: /Tasks
        public ActionResult Index(int? projectId, TaskStatus? statusFilter, TaskPriority? priorityFilter, bool overdueOnly = false)
        {
            string currentUserId = User.Identity.GetUserId();
            IQueryable<TaskItem> query = _db.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser);

            if (User.IsInRole("Admin"))
            {
                // Admin sees all
            }
            else if (User.IsInRole("ProjectManager"))
            {
                query = query.Where(t => t.Project.ProjectManagerId == currentUserId);
            }
            else // Normal User
            {
                query = query.Where(t => t.AssignedToUserId == currentUserId);
            }

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }
            if (statusFilter.HasValue)
            {
                query = query.Where(t => t.Status == statusFilter.Value);
            }
            if (priorityFilter.HasValue)
            {
                query = query.Where(t => t.Priority == priorityFilter.Value);
            }
            if (overdueOnly)
            {
                query = query.Where(t => t.DueDate < DateTime.Now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);
            }

            var tasks = query.OrderByDescending(t => t.CreatedDate).ToList();

            ViewBag.ProjectId = projectId;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.PriorityFilter = priorityFilter;
            ViewBag.OverdueOnly = overdueOnly;
            ViewBag.ProjectName = projectId.HasValue ? _db.Projects.Find(projectId.Value)?.Name : null;

            return View(tasks);
        }

        // GET: /Tasks/Details/5
        public ActionResult Details(int id)
        {
            var taskItem = _db.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Comments.Select(c => c.User))
                .Include(t => t.Files.Select(f => f.UploadedByUser))
                .FirstOrDefault(t => t.TaskItemId == id);

            if (taskItem == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && taskItem.Project.ProjectManagerId == currentUserId;
            bool isAssignee = taskItem.AssignedToUserId == currentUserId;

            var model = new TaskDetailsViewModel
            {
                Task = taskItem,
                Comments = taskItem.Comments.OrderByDescending(c => c.CreatedDate).ToList(),
                Files = taskItem.Files.OrderByDescending(f => f.UploadedDate).ToList(),
                CanEditTask = isAdmin || isPM,
                CanUpdateStatus = isAdmin || isPM || isAssignee
            };

            return View(model);
        }

        // GET: /Tasks/Create
        public ActionResult Create(int? projectId)
        {
            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager");

            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            var model = new TaskFormViewModel
            {
                ProjectId = projectId ?? 0,
                DueDate = DateTime.Today.AddDays(7),
                ProjectList = GetProjectSelectList(currentUserId, isAdmin, projectId),
                AssigneeList = GetAssigneeSelectList(projectId)
            };

            return View(model);
        }

        // POST: /Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TaskFormViewModel model)
        {
            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager");

            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                model.ProjectList = GetProjectSelectList(currentUserId, isAdmin, model.ProjectId);
                model.AssigneeList = GetAssigneeSelectList(model.ProjectId, model.AssignedToUserId);
                return View(model);
            }

            var taskItem = new TaskItem
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                AssignedToUserId = model.AssignedToUserId,
                CreatedByUserId = currentUserId,
                Priority = model.Priority,
                Status = model.Status,
                ProgressPercentage = model.ProgressPercentage,
                StartDate = model.StartDate,
                DueDate = model.DueDate,
                CreatedDate = DateTime.Now
            };

            _db.TaskItems.Add(taskItem);
            await _db.SaveChangesAsync();

            // Create notification for assigned user
            _db.Notifications.Add(new Notification
            {
                UserId = model.AssignedToUserId,
                ProjectId = model.ProjectId,
                TaskItemId = taskItem.TaskItemId,
                NotificationType = "TaskAssigned",
                Title = "New Task Assigned",
                Message = $"You have been assigned to task '{taskItem.Title}'. Deadline: {taskItem.DueDate.ToString("yyyy-MM-dd")}",
                IsRead = false,
                CreatedDate = DateTime.Now
            });

            // Log Email Notification Queue (ready for mail service API)
            var assignedUser = _db.Users.Find(model.AssignedToUserId);
            if (assignedUser != null)
            {
                _db.EmailLogs.Add(new EmailLog
                {
                    RecipientEmail = assignedUser.Email,
                    Subject = $"[Task Assigned] {taskItem.Title}",
                    Body = $"Hello {assignedUser.FirstName},\n\nYou have been assigned a new task '{taskItem.Title}' due on {taskItem.DueDate:yyyy-MM-dd}.\n\nPriority: {taskItem.Priority}",
                    EmailType = "TaskAssignmentNotification",
                    RelatedUserId = assignedUser.Id,
                    RelatedProjectId = model.ProjectId,
                    RelatedTaskItemId = taskItem.TaskItemId,
                    Status = "Queued",
                    CreatedDate = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Task '{taskItem.Title}' created and assigned successfully!";
            return RedirectToAction("Details", new { id = taskItem.TaskItemId });
        }

        // GET: /Tasks/Edit/5
        public ActionResult Edit(int id)
        {
            var taskItem = _db.TaskItems.Include(t => t.Project).FirstOrDefault(t => t.TaskItemId == id);
            if (taskItem == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && taskItem.Project.ProjectManagerId == currentUserId;

            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            var model = new TaskFormViewModel
            {
                TaskItemId = taskItem.TaskItemId,
                ProjectId = taskItem.ProjectId,
                Title = taskItem.Title,
                Description = taskItem.Description,
                AssignedToUserId = taskItem.AssignedToUserId,
                Priority = taskItem.Priority,
                Status = taskItem.Status,
                ProgressPercentage = taskItem.ProgressPercentage,
                StartDate = taskItem.StartDate,
                DueDate = taskItem.DueDate,
                ProjectList = GetProjectSelectList(currentUserId, isAdmin, taskItem.ProjectId),
                AssigneeList = GetAssigneeSelectList(taskItem.ProjectId, taskItem.AssignedToUserId)
            };

            return View(model);
        }

        // POST: /Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TaskFormViewModel model)
        {
            var taskItem = _db.TaskItems.Include(t => t.Project).FirstOrDefault(t => t.TaskItemId == model.TaskItemId);
            if (taskItem == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && taskItem.Project.ProjectManagerId == currentUserId;

            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                model.ProjectList = GetProjectSelectList(currentUserId, isAdmin, model.ProjectId);
                model.AssigneeList = GetAssigneeSelectList(model.ProjectId, model.AssignedToUserId);
                return View(model);
            }

            taskItem.Title = model.Title;
            taskItem.Description = model.Description;
            taskItem.AssignedToUserId = model.AssignedToUserId;
            taskItem.Priority = model.Priority;
            taskItem.Status = model.Status;
            taskItem.ProgressPercentage = model.ProgressPercentage;
            taskItem.StartDate = model.StartDate;
            taskItem.DueDate = model.DueDate;
            taskItem.UpdatedDate = DateTime.Now;

            if (taskItem.Status == TaskStatus.Completed && !taskItem.CompletedDate.HasValue)
            {
                taskItem.CompletedDate = DateTime.Now;
                taskItem.ProgressPercentage = 100;
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Task '{taskItem.Title}' updated successfully!";
            return RedirectToAction("Details", new { id = taskItem.TaskItemId });
        }

        // POST: /Tasks/UpdateStatusProgress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStatusProgress(TaskStatusUpdateViewModel model)
        {
            var taskItem = _db.TaskItems.Include(t => t.Project).FirstOrDefault(t => t.TaskItemId == model.TaskItemId);
            if (taskItem == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && taskItem.Project.ProjectManagerId == currentUserId;
            bool isAssignee = taskItem.AssignedToUserId == currentUserId;

            if (!isAdmin && !isPM && !isAssignee) return RedirectToAction("AccessDenied", "Account");

            taskItem.Status = model.Status;
            taskItem.ProgressPercentage = model.ProgressPercentage;
            taskItem.UpdatedDate = DateTime.Now;

            if (model.Status == TaskStatus.Completed)
            {
                taskItem.CompletedDate = DateTime.Now;
                taskItem.ProgressPercentage = 100;
            }

            if (!string.IsNullOrEmpty(model.Comment))
            {
                _db.TaskComments.Add(new TaskComment
                {
                    TaskItemId = taskItem.TaskItemId,
                    UserId = currentUserId,
                    CommentText = $"[Status Changed to {model.Status} - {model.ProgressPercentage}% Progress]: {model.Comment}",
                    CreatedDate = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task status and progress updated!";
            return RedirectToAction("Details", new { id = taskItem.TaskItemId });
        }

        // POST: /Tasks/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddComment(int taskItemId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["ErrorMessage"] = "Comment text cannot be empty.";
                return RedirectToAction("Details", new { id = taskItemId });
            }

            string currentUserId = User.Identity.GetUserId();
            var comment = new TaskComment
            {
                TaskItemId = taskItemId,
                UserId = currentUserId,
                CommentText = commentText.Trim(),
                CreatedDate = DateTime.Now
            };

            _db.TaskComments.Add(comment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Comment added successfully!";
            return RedirectToAction("Details", new { id = taskItemId });
        }

        // POST: /Tasks/UploadFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFile(int taskItemId, HttpPostedFileBase uploadedFile)
        {
            if (uploadedFile == null || uploadedFile.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid file to upload.";
                return RedirectToAction("Details", new { id = taskItemId });
            }

            string currentUserId = User.Identity.GetUserId();
            string uploadsFolder = Server.MapPath("~/App_Data/Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string originalFileName = Path.GetFileName(uploadedFile.FileName);
            string fileExt = Path.GetExtension(originalFileName);
            string storedFileName = $"{Guid.NewGuid()}{fileExt}";
            string savePath = Path.Combine(uploadsFolder, storedFileName);

            uploadedFile.SaveAs(savePath);

            var taskFile = new TaskFile
            {
                TaskItemId = taskItemId,
                UploadedByUserId = currentUserId,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                FilePath = $"/App_Data/Uploads/{storedFileName}",
                FileSize = uploadedFile.ContentLength,
                ContentType = uploadedFile.ContentType,
                UploadedDate = DateTime.Now
            };

            _db.TaskFiles.Add(taskFile);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "File uploaded successfully!";
            return RedirectToAction("Details", new { id = taskItemId });
        }

        private IEnumerable<SelectListItem> GetProjectSelectList(string currentUserId, bool isAdmin, int? selectedProjectId)
        {
            IQueryable<Project> query = _db.Projects.Where(p => p.Status != ProjectStatus.Cancelled);
            if (!isAdmin)
            {
                query = query.Where(p => p.ProjectManagerId == currentUserId);
            }

            return query.ToList().Select(p => new SelectListItem
            {
                Value = p.ProjectId.ToString(),
                Text = p.Name,
                Selected = (p.ProjectId == selectedProjectId)
            });
        }

        private IEnumerable<SelectListItem> GetAssigneeSelectList(int? projectId, string selectedUserId = null)
        {
            List<ApplicationUser> users;
            if (projectId.HasValue && projectId.Value > 0)
            {
                var memberUserIds = _db.ProjectMembers.Where(pm => pm.ProjectId == projectId.Value && pm.IsActive).Select(pm => pm.UserId).ToList();
                users = _db.Users.Where(u => u.IsActive && memberUserIds.Contains(u.Id)).ToList();
            }
            else
            {
                users = _db.Users.Where(u => u.IsActive).ToList();
            }

            return users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FirstName} {u.LastName} ({u.Email})",
                Selected = (u.Id == selectedUserId)
            });
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
