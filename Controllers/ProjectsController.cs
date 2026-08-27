using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.Enums;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProjectsController()
        {
            _db = new ApplicationDbContext();
        }

        // GET: /Projects
        public ActionResult Index(ProjectStatus? statusFilter)
        {
            string currentUserId = User.Identity.GetUserId();
            IQueryable<Project> query = _db.Projects.Include(p => p.ProjectManager).Include(p => p.Department).Include(p => p.Tasks);

            if (User.IsInRole("Admin"))
            {
                // Admin sees all
            }
            else if (User.IsInRole("ProjectManager"))
            {
                query = query.Where(p => p.ProjectManagerId == currentUserId);
            }
            else // Normal User
            {
                var memberProjectIds = _db.ProjectMembers.Where(pm => pm.UserId == currentUserId && pm.IsActive).Select(pm => pm.ProjectId);
                query = query.Where(p => memberProjectIds.Contains(p.ProjectId));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(p => p.Status == statusFilter.Value);
            }

            var projects = query.OrderByDescending(p => p.CreatedDate).ToList();
            ViewBag.StatusFilter = statusFilter;
            return View(projects);
        }

        // GET: /Projects/Details/5
        public ActionResult Details(int id)
        {
            var project = _db.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Department)
                .Include(p => p.Members.Select(m => m.User))
                .Include(p => p.Tasks.Select(t => t.AssignedToUser))
                .FirstOrDefault(p => p.ProjectId == id);

            if (project == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && project.ProjectManagerId == currentUserId;
            bool isMember = project.Members.Any(m => m.UserId == currentUserId && m.IsActive);

            if (!isAdmin && !isPM && !isMember)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new ProjectDetailsViewModel
            {
                Project = project,
                Members = project.Members.Where(m => m.IsActive).ToList(),
                Tasks = project.Tasks.OrderByDescending(t => t.CreatedDate).ToList(),
                CanEditProject = isAdmin || isPM,
                CanManageMembers = isAdmin || isPM,
                CanCreateTask = isAdmin || isPM
            };

            return View(model);
        }

        // GET: /Projects/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var model = new ProjectFormViewModel
            {
                StartDate = DateTime.Today,
                DepartmentList = GetDepartmentSelectList(),
                ProjectManagerList = GetProjectManagerSelectList()
            };
            return View(model);
        }

        // POST: /Projects/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProjectFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DepartmentList = GetDepartmentSelectList(model.DepartmentId);
                model.ProjectManagerList = GetProjectManagerSelectList(model.ProjectManagerId);
                return View(model);
            }

            string currentUserId = User.Identity.GetUserId();
            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = model.Status,
                ProgressPercentage = 0,
                DepartmentId = model.DepartmentId,
                ProjectManagerId = model.ProjectManagerId,
                CreatedByUserId = currentUserId,
                CreatedDate = DateTime.Now
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            // Automatically add PM as a ProjectMember
            _db.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = project.ProjectId,
                UserId = model.ProjectManagerId,
                JoinedDate = DateTime.Now,
                IsActive = true
            });
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Project '{project.Name}' created successfully!";
            return RedirectToAction("Details", new { id = project.ProjectId });
        }

        // GET: /Projects/Edit/5
        public ActionResult Edit(int id)
        {
            var project = _db.Projects.FirstOrDefault(p => p.ProjectId == id);
            if (project == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && project.ProjectManagerId == currentUserId;

            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            var model = new ProjectFormViewModel
            {
                ProjectId = project.ProjectId,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage,
                DepartmentId = project.DepartmentId,
                ProjectManagerId = project.ProjectManagerId,
                DepartmentList = GetDepartmentSelectList(project.DepartmentId),
                ProjectManagerList = GetProjectManagerSelectList(project.ProjectManagerId)
            };

            return View(model);
        }

        // POST: /Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProjectFormViewModel model)
        {
            var project = _db.Projects.FirstOrDefault(p => p.ProjectId == model.ProjectId);
            if (project == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && project.ProjectManagerId == currentUserId;
            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                model.DepartmentList = GetDepartmentSelectList(model.DepartmentId);
                model.ProjectManagerList = GetProjectManagerSelectList(model.ProjectManagerId);
                return View(model);
            }

            project.Name = model.Name;
            project.Description = model.Description;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.Status = model.Status;
            project.ProgressPercentage = model.ProgressPercentage;
            project.UpdatedDate = DateTime.Now;

            if (isAdmin)
            {
                project.DepartmentId = model.DepartmentId;
                project.ProjectManagerId = model.ProjectManagerId;
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Project '{project.Name}' updated successfully!";
            return RedirectToAction("Details", new { id = project.ProjectId });
        }

        // GET: /Projects/ManageMembers/5
        public ActionResult ManageMembers(int id)
        {
            var project = _db.Projects.Include(p => p.Members.Select(m => m.User)).FirstOrDefault(p => p.ProjectId == id);
            if (project == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && project.ProjectManagerId == currentUserId;
            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            var currentMemberUserIds = project.Members.Where(m => m.IsActive).Select(m => m.UserId).ToList();
            var availableUsers = _db.Users.Where(u => u.IsActive && !currentMemberUserIds.Contains(u.Id)).ToList();

            var model = new ProjectMemberViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.Name,
                CurrentMembers = project.Members.Where(m => m.IsActive).ToList(),
                AvailableUsersList = availableUsers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddMember(int projectId, string selectedUserId)
        {
            var project = _db.Projects
                .FirstOrDefault(p => p.ProjectId == projectId);

            if (project == null)
                return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();

            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") &&
                        project.ProjectManagerId == currentUserId;

            if (!isAdmin && !isPM)
                return RedirectToAction("AccessDenied", "Account");

            // Make sure a user was selected
            if (string.IsNullOrWhiteSpace(selectedUserId))
            {
                TempData["ErrorMessage"] = "Please select a user to add.";
                return RedirectToAction("ManageMembers", new { id = projectId });
            }

            // Make sure the selected user exists
            var user = _db.Users.FirstOrDefault(u => u.Id == selectedUserId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "The selected user was not found.";
                return RedirectToAction("ManageMembers", new { id = projectId });
            }

            // Check whether the user is already a member
            var existingMember = _db.ProjectMembers
                .FirstOrDefault(pm =>
                    pm.ProjectId == projectId &&
                    pm.UserId == selectedUserId);

            if (existingMember != null)
            {
                existingMember.IsActive = true;
            }
            else
            {
                _db.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = selectedUserId,
                    JoinedDate = DateTime.Now,
                    IsActive = true
                });
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Team member added successfully!";

            return RedirectToAction(
                "ManageMembers",
                new { id = projectId }
            );
        }

        // POST: /Projects/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveMember(int projectMemberId)
        {
            var member = _db.ProjectMembers.Include(m => m.Project).FirstOrDefault(m => m.ProjectMemberId == projectMemberId);
            if (member == null) return HttpNotFound();

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isPM = User.IsInRole("ProjectManager") && member.Project.ProjectManagerId == currentUserId;
            if (!isAdmin && !isPM) return RedirectToAction("AccessDenied", "Account");

            member.IsActive = false;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Team member removed successfully!";
            return RedirectToAction("ManageMembers", new { id = member.ProjectId });
        }

        private IEnumerable<SelectListItem> GetDepartmentSelectList(int? selectedDeptId = null)
        {
            return _db.Departments.Where(d => d.IsActive).Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name,
                Selected = (d.DepartmentId == selectedDeptId)
            }).ToList();
        }

        private IEnumerable<SelectListItem> GetProjectManagerSelectList(string selectedPmId = null)
        {
            var pmRoleId = _db.Roles.FirstOrDefault(r => r.Name == "ProjectManager")?.Id;
            var pmUserIds = _db.Users.Where(u => u.Roles.Any(r => r.RoleId == pmRoleId) && u.IsActive).Select(u => u.Id).ToList();
            var pmUsers = _db.Users.Where(u => pmUserIds.Contains(u.Id) || u.Id == selectedPmId).ToList();

            return pmUsers.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FirstName} {u.LastName} ({u.Email})",
                Selected = (u.Id == selectedPmId)
            }).ToList();
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
