using FirstBloom.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // ALL CONTACT ENQUIRIES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(messages);
        }


        // =========================================================
        // VIEW ENQUIRY DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            // Mark message as read when admin opens it
            message.IsRead = true;

            await _context.SaveChangesAsync();

            return View(message);
        }


        // =========================================================
        // MARK AS READ
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            message.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // MARK AS UNREAD
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsUnread(int id)
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            message.IsRead = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE ENQUIRY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            _context.ContactMessages.Remove(message);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Enquiry deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}