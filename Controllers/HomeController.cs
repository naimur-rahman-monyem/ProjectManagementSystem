using System.Web.Mvc;

namespace ProjectManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            // If user is already logged in,
            // send them directly to the dashboard.
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }


        // GET: Home/About
        public ActionResult About()
        {
            ViewBag.Message = "About our Project Management System.";

            return View();
        }


        // GET: Home/Contact
        public ActionResult Contact()
        {
            ViewBag.Message = "We would love to hear from you.";

            return View();
        }


        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(
            string name,
            string email,
            string subject,
            string message)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(subject) ||
                string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] =
                    "Please fill in all fields.";

                return RedirectToAction("Contact");
            }

            // TODO:
            // Add your email/database logic here.
            //
            // Example:
            // Save the message to a ContactMessage table
            // or send it through an email service.

            TempData["SuccessMessage"] =
                "Thank you! Your message has been sent successfully.";

            return RedirectToAction("Contact");
        }
    }
}