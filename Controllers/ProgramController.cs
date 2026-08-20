using FirstBloom.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Controllers
{
    public class ProgramController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgramController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Programs
        public async Task<IActionResult> Index()
        {
            var programs = await _context.Programs
                .Where(p => p.IsActive)
                .OrderBy(p => p.Id)
                .ToListAsync();

            return View(programs);
        }

        // GET: /Programs/Details/1
        public async Task<IActionResult> Details(int id)
        {
            var program = await _context.Programs
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }
    }
}