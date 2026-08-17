using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstBloom.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Contact
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            model.IsRead = false;

            _context.ContactMessages.Add(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Thank you for contacting FirstBloom Academy! We will get back to you soon.";

            return RedirectToAction(nameof(Index));
        }
    }
}