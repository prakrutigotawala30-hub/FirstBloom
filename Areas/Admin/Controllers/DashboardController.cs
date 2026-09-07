using FirstBloom.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //ViewBag.TotalAdmissions =
            //    await _context.Admissions.CountAsync();

            //ViewBag.TotalStudents =
            //    await _context.Students.CountAsync();

            ViewBag.TotalPrograms =
                await _context.Programs.CountAsync();

            ViewBag.TotalEnquiries =
                await _context.ContactMessages.CountAsync();

            return View();
        }
    }
}