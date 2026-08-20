using FirstBloom.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GalleryController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET: /Gallery
        // =========================================

        public async Task<IActionResult> Index(string category)
        {
            var galleries = _context.Galleries
                .AsQueryable();

            // Filter by category
            if (!string.IsNullOrEmpty(category) &&
                category != "All")
            {
                galleries = galleries
                    .Where(x => x.Category == category);
            }

            var result = await galleries
                .OrderByDescending(x => x.CreatedAt)
                .Take(9)
                .ToListAsync();

            ViewBag.SelectedCategory = category ?? "All";

            return View(result);
        }


        // =========================================
        // GET: /Gallery/Details/1
        // =========================================

        public async Task<IActionResult> Details(int id)
        {
            var gallery = await _context.Galleries
                .FirstOrDefaultAsync(x => x.Id == id);

            if (gallery == null)
            {
                return NotFound();
            }

            return View(gallery);
        }
    }
}