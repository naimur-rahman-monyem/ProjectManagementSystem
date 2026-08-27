using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NotificationsController()
        {
            _db = new ApplicationDbContext();
        }

        // GET: /Notifications
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();
            var notifications = _db.Notifications
                .Include(n => n.Project)
                .Include(n => n.TaskItem)
                .Where(n => n.UserId == currentUserId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();

            return View(notifications);
        }

        // POST: /Notifications/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            string currentUserId = User.Identity.GetUserId();
            var notification = _db.Notifications.FirstOrDefault(n => n.NotificationId == id && n.UserId == currentUserId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadDate = System.DateTime.Now;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: /Notifications/UnreadCount (Partial or JSON)
        public JsonResult UnreadCount()
        {
            string currentUserId = User.Identity.GetUserId();
            int count = _db.Notifications.Count(n => n.UserId == currentUserId && !n.IsRead);
            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
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
