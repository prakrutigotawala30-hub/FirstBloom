using FirstBloom.Data;
using FirstBloom.Models.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FirstBloom.Controllers
{
    [Authorize]
    public class StudentDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentDashboardController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "StudentAccount"
                );
            }

            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (student == null)
            {
                return NotFound(
                    "Student profile was not found."
                );
            }

            return View(student);
        }
    }
}