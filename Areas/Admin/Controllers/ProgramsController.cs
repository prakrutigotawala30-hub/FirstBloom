using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Programs
        public async Task<IActionResult> Index()
        {
            var programs = await _context.Programs
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(programs);
        }


        // GET: Admin/Programs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var program = await _context.Programs
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
                return NotFound();

            return View(program);
        }


        // GET: Admin/Programs/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Admin/Programs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Programs program)
        {
            if (ModelState.IsValid)
            {
                program.CreatedAt = DateTime.Now;

                _context.Programs.Add(program);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Program created successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(program);
        }


        // GET: Admin/Programs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var program = await _context.Programs.FindAsync(id);

            if (program == null)
                return NotFound();

            return View(program);
        }


        // POST: Admin/Programs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Programs program)
        {
            if (id != program.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProgram = await _context.Programs
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProgram == null)
                        return NotFound();

                    existingProgram.ProgramName = program.ProgramName;
                    existingProgram.AgeGroup = program.AgeGroup;
                    existingProgram.Duration = program.Duration;
                    existingProgram.Fee = program.Fee;
                    existingProgram.Description = program.Description;
                    existingProgram.ImageUrl = program.ImageUrl;
                    existingProgram.IsActive = program.IsActive;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Program updated successfully.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProgramExists(program.Id))
                        return NotFound();

                    throw;
                }
            }

            return View(program);
        }


        // GET: Admin/Programs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var program = await _context.Programs
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
                return NotFound();

            return View(program);
        }


        // POST: Admin/Programs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var program = await _context.Programs.FindAsync(id);

            if (program == null)
                return NotFound();

            _context.Programs.Remove(program);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Program deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        private bool ProgramExists(int id)
        {
            return _context.Programs.Any(p => p.Id == id);
        }
    }
}