using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SystemSettingsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SystemSettingsController()
        {
            _db = new ApplicationDbContext();
        }

        // GET: /SystemSettings
        public ActionResult Index()
        {
            var model = new SystemSettingsViewModel
            {
                Settings = _db.SystemSettings.OrderBy(s => s.SettingKey).ToList(),
                RecentEmailLogs = _db.EmailLogs.OrderByDescending(e => e.CreatedDate).Take(20).ToList(),
                RecentAuditLogs = _db.AuditLogs.Include(a => a.User).OrderByDescending(a => a.CreatedDate).Take(20).ToList()
            };

            return View(model);
        }

        // POST: /SystemSettings/UpdateSetting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateSetting(int systemSettingId, string settingValue)
        {
            var setting = await _db.SystemSettings.FindAsync(systemSettingId);
            if (setting != null)
            {
                setting.SettingValue = settingValue;
                setting.UpdatedDate = DateTime.Now;
                setting.UpdatedByUserId = User.Identity.GetUserId();
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Setting '{setting.SettingKey}' updated!";
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
