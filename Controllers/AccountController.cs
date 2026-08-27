using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login credentials.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account has been deactivated. Please contact System Administrator.");
                return View(model);
            }

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Invalid login credentials.");
                return View(model);
            }

            // Record Last Login Date
            user.LastLoginDate = System.DateTime.Now;
            await _userManager.UpdateAsync(user);

            // Create Identity & Sign In
            var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, identity);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "User with this email address already exists.");
                return View(model);
            }

            var defaultDept = _db.Departments.FirstOrDefault(d => d.IsActive);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DepartmentId = defaultDept?.DepartmentId,
                IsActive = true,
                CreatedDate = System.DateTime.Now,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user.Id, "User");

                var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                TempData["SuccessMessage"] = "Account registered successfully! Welcome to Project Management System.";
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }

        // GET: /Account/Profile
        [Authorize]
        [ActionName("Profile")]
        public ActionResult UserProfile()
        {
            string currentUserId = User.Identity.GetUserId();
            var user = _db.Users.Include(u => u.Department).FirstOrDefault(u => u.Id == currentUserId);
            if (user == null) return HttpNotFound();

            var userRoles = _userManager.GetRoles(user.Id);
            string roleName = userRoles.FirstOrDefault() ?? "User";

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleName = roleName,
                DepartmentName = user.Department?.Name ?? "No Department"
            };

            return View("Profile", model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ActionName("Profile")]
        public async Task<ActionResult> UserProfile(UserProfileViewModel model)
        {
            string currentUserId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return HttpNotFound();

            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            await _userManager.UpdateAsync(user);

            // Password Change Logic if provided
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return View("Profile", model);
                }
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: /Account/AccessDenied
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
