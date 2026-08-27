using System.Web.Mvc;

namespace ProjectManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Return the Razor view containing your landing page
            return View(); 
        }
    }
}