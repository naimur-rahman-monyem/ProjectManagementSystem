using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_db));
        }

        // GET: /Users
        public ActionResult Index()
        {
            var users = _db.Users.Include(u => u.Department).ToList();
            var userViewModels = new List<UserListViewModel>();

            foreach (var u in users)
            {
                var roles = _userManager.GetRoles(u.Id);
                userViewModels.Add(new UserListViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    RoleName = roles.FirstOrDefault() ?? "None",
                    DepartmentName = u.Department?.Name ?? "Unassigned",
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    LastLoginDate = u.LastLoginDate
                });
            }

            return View(userViewModels);
        }

        // GET: /Users/Create
        public ActionResult Create()
        {
            var model = new UserCreateViewModel
            {
                RoleList = GetRoleSelectList(),
                DepartmentList = GetDepartmentSelectList()
            };
            return View(model);
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RoleList = GetRoleSelectList();
                model.DepartmentList = GetDepartmentSelectList();
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "User with this email address already exists.");
                model.RoleList = GetRoleSelectList();
                model.DepartmentList = GetDepartmentSelectList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DepartmentId = model.DepartmentId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.RoleName))
                {
                    await _userManager.AddToRoleAsync(user.Id, model.RoleName);
                }

                // Log audit action
                _db.AuditLogs.Add(new AuditLog
                {
                    UserId = User.Identity.GetUserId(),
                    Action = "Create User",
                    EntityType = "ApplicationUser",
                    EntityId = user.Id,
                    NewValue = $"Created user {user.Email} with role {model.RoleName}",
                    CreatedDate = DateTime.Now
                });
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User {user.Email} created successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            model.RoleList = GetRoleSelectList();
            model.DepartmentList = GetDepartmentSelectList();
            return View(model);
        }

        // GET: /Users/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return HttpNotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            var currentRoles = await _userManager.GetRolesAsync(id);
            string roleName = currentRoles.FirstOrDefault() ?? "User";

            var model = new UserEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DepartmentId = user.DepartmentId,
                RoleName = roleName,
                IsActive = user.IsActive,
                RoleList = GetRoleSelectList(roleName),
                DepartmentList = GetDepartmentSelectList(user.DepartmentId)
            };

            return View(model);
        }

        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RoleList = GetRoleSelectList(model.RoleName);
                model.DepartmentList = GetDepartmentSelectList(model.DepartmentId);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return HttpNotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.DepartmentId = model.DepartmentId;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors) ModelState.AddModelError("", error);
                model.RoleList = GetRoleSelectList(model.RoleName);
                model.DepartmentList = GetDepartmentSelectList(model.DepartmentId);
                return View(model);
            }

            // Update Role
            var currentRoles = await _userManager.GetRolesAsync(user.Id);
            string currentRole = currentRoles.FirstOrDefault();
            if (currentRole != model.RoleName)
            {
                if (!string.IsNullOrEmpty(currentRole))
                {
                    await _userManager.RemoveFromRoleAsync(user.Id, currentRole);
                }
                if (!string.IsNullOrEmpty(model.RoleName))
                {
                    await _userManager.AddToRoleAsync(user.Id, model.RoleName);
                }
            }

            // Password update if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                await _userManager.ResetPasswordAsync(user.Id, token, model.NewPassword);
            }

            TempData["SuccessMessage"] = $"User {user.Email} updated successfully!";
            return RedirectToAction("Index");
        }

        // POST: /Users/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"User {user.Email} status updated to {(user.IsActive ? "Active" : "Inactive")}.";
            }
            return RedirectToAction("Index");
        }

        private IEnumerable<SelectListItem> GetRoleSelectList(string selectedRole = null)
        {
            var roles = _roleManager.Roles.ToList();
            return roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = (r.Name == selectedRole)
            });
        }

        private IEnumerable<SelectListItem> GetDepartmentSelectList(int? selectedDeptId = null)
        {
            var depts = _db.Departments.Where(d => d.IsActive).ToList();
            return depts.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name,
                Selected = (d.DepartmentId == selectedDeptId)
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
                _userManager.Dispose();
                _roleManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
