using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Controllers
{
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var about = await _context.Abouts
                .FirstOrDefaultAsync(a => a.IsActive);

            if (about == null)
            {
                about = new About
                {
                    AcademyName = "FirstBloom Academy",
                    Title = "About Us",
                    Description = "FirstBloom Academy provides quality education in a safe, caring and creative environment where every child is encouraged to learn and grow.",
                    Mission = "To provide quality education and build strong values.",
                    Vision = "To inspire every child to become confident and responsible.",
                    Story = "FirstBloom Academy was established to provide excellent early childhood education.",
                    PrincipalName = "Mrs. Prakruti Gotawala",
                    PrincipalMessage = "Every child deserves the opportunity to learn, explore and succeed.",
                    AboutImage = "/images/about/about.jpg",
                    PrincipalImage = "/images/about/principal.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Abouts.Add(about);
                await _context.SaveChangesAsync();
            }

            return View(about);
        }
    }
}