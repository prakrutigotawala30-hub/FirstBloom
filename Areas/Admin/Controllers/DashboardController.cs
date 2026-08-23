using Microsoft.AspNetCore.Mvc;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.TotalAdmissions = 256;
            ViewBag.TotalStudents = 1248;
            ViewBag.TotalPrograms = 18;
            ViewBag.TotalEnquiries = 356;

            return View();
        }
    }
}