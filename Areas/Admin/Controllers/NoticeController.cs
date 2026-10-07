using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NoticeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NoticeController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // NOTICE LIST
        // GET: /Admin/Notice
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var notices = await _context.Notices
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(notices);
        }


        // =========================================================
        // CREATE NOTICE - GET
        // GET: /Admin/Notice/Create
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Notice
            {
                IsActive = true
            });
        }


        // =========================================================
        // CREATE NOTICE - POST
        // POST: /Admin/Notice/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notice model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.Now;

            if (model.IsActive)
            {
                model.PublishedAt = DateTime.Now;
            }

            _context.Notices.Add(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Notice created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT NOTICE - GET
        // GET: /Admin/Notice/Edit/5
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var notice = await _context.Notices
                .FirstOrDefaultAsync(x => x.Id == id);

            if (notice == null)
            {
                return NotFound();
            }

            return View(notice);
        }


        // =========================================================
        // EDIT NOTICE - POST
        // POST: /Admin/Notice/Edit
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Notice model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var notice = await _context.Notices
                .FirstOrDefaultAsync(x => x.Id == id);

            if (notice == null)
            {
                return NotFound();
            }

            notice.Title = model.Title;
            notice.Message = model.Message;

            // Publish date
            if (model.IsActive && !notice.IsActive)
            {
                notice.PublishedAt = DateTime.Now;
            }

            if (!model.IsActive)
            {
                notice.PublishedAt = null;
            }

            notice.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Notice updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // TOGGLE NOTICE STATUS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var notice = await _context.Notices
                .FirstOrDefaultAsync(x => x.Id == id);

            if (notice == null)
            {
                return NotFound();
            }

            notice.IsActive = !notice.IsActive;

            if (notice.IsActive)
            {
                notice.PublishedAt = DateTime.Now;
            }
            else
            {
                notice.PublishedAt = null;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                notice.IsActive
                    ? "Notice published successfully."
                    : "Notice unpublished successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE NOTICE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notice = await _context.Notices
                .FirstOrDefaultAsync(x => x.Id == id);

            if (notice == null)
            {
                return NotFound();
            }

            _context.Notices.Remove(notice);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Notice deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}