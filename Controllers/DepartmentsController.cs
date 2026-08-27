using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DepartmentsController()
        {
            _db = new ApplicationDbContext();
        }

        // GET: /Departments
        public ActionResult Index()
        {
            var depts = _db.Departments.Include(d => d.Users).Include(d => d.Projects).ToList();
            return View(depts);
        }

        // GET: /Departments/Create
        public ActionResult Create()
        {
            var model = new DepartmentFormViewModel();
            return View(model);
        }

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DepartmentFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dept = new Department
            {
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive
            };

            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Department '{dept.Name}' created successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Departments/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var dept = await _db.Departments.FindAsync(id);
            if (dept == null) return HttpNotFound();

            var model = new DepartmentFormViewModel
            {
                DepartmentId = dept.DepartmentId,
                Name = dept.Name,
                Description = dept.Description,
                IsActive = dept.IsActive
            };

            return View(model);
        }

        // POST: /Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DepartmentFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dept = await _db.Departments.FindAsync(model.DepartmentId);
            if (dept == null) return HttpNotFound();

            dept.Name = model.Name;
            dept.Description = model.Description;
            dept.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Department '{dept.Name}' updated successfully!";
            return RedirectToAction("Index");
        }

        // POST: /Departments/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleStatus(int id)
        {
            var dept = await _db.Departments.FindAsync(id);
            if (dept != null)
            {
                dept.IsActive = !dept.IsActive;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Department status updated to {(dept.IsActive ? "Active" : "Inactive")}.";
            }
            return RedirectToAction("Index");
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
