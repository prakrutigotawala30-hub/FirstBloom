using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AcademicYearsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcademicYearsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: /Admin/AcademicYears
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var academicYears = await _context.AcademicYears
                .OrderByDescending(x => x.IsCurrent)
                .ThenByDescending(x => x.StartDate)
                .ToListAsync();

            return View(academicYears);
        }


        // =========================================================
        // GET: /Admin/AcademicYears/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicYear = await _context.AcademicYears
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (academicYear == null)
            {
                return NotFound();
            }

            return View(academicYear);
        }


        // =========================================================
        // GET: /Admin/AcademicYears/Create
        // =========================================================
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // POST: /Admin/AcademicYears/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcademicYear academicYear)
        {
            // -----------------------------------------------------
            // Validate Start Date and End Date
            // -----------------------------------------------------
            if (academicYear.StartDate >= academicYear.EndDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date must be after the start date."
                );
            }


            // -----------------------------------------------------
            // Prevent duplicate academic year name
            // Example: 2026-27 cannot be entered twice
            // -----------------------------------------------------
            bool duplicateName = await _context.AcademicYears
                .AnyAsync(x =>
                    x.Name.ToLower() == academicYear.Name.ToLower());

            if (duplicateName)
            {
                ModelState.AddModelError(
                    "Name",
                    "This academic year already exists."
                );
            }


            // -----------------------------------------------------
            // Prevent overlapping academic years
            // -----------------------------------------------------
            bool overlappingYear = await _context.AcademicYears
                .AnyAsync(x =>
                    academicYear.StartDate <= x.EndDate &&
                    academicYear.EndDate >= x.StartDate);

            if (overlappingYear)
            {
                ModelState.AddModelError(
                    "",
                    "The selected dates overlap with an existing academic year."
                );
            }


            // -----------------------------------------------------
            // Return form if validation failed
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return View(academicYear);
            }


            // -----------------------------------------------------
            // If this is current year,
            // automatically make it active.
            // -----------------------------------------------------
            if (academicYear.IsCurrent)
            {
                academicYear.IsActive = true;

                var currentYears = await _context.AcademicYears
                    .Where(x => x.IsCurrent)
                    .ToListAsync();

                foreach (var year in currentYears)
                {
                    year.IsCurrent = false;
                }
            }


            // -----------------------------------------------------
            // Created date
            // -----------------------------------------------------
            academicYear.CreatedAt = DateTime.Now;


            // -----------------------------------------------------
            // Save
            // -----------------------------------------------------
            _context.AcademicYears.Add(academicYear);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Academic year created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: /Admin/AcademicYears/Edit/5
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicYear = await _context.AcademicYears
                .FindAsync(id);

            if (academicYear == null)
            {
                return NotFound();
            }

            return View(academicYear);
        }


        // =========================================================
        // POST: /Admin/AcademicYears/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            AcademicYear model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Validate dates
            // -----------------------------------------------------
            if (model.StartDate >= model.EndDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date must be after the start date."
                );
            }


            // -----------------------------------------------------
            // Check duplicate academic year name
            // Exclude current record
            // -----------------------------------------------------
            bool duplicateName = await _context.AcademicYears
                .AnyAsync(x =>
                    x.Id != model.Id &&
                    x.Name.ToLower() == model.Name.ToLower());

            if (duplicateName)
            {
                ModelState.AddModelError(
                    "Name",
                    "This academic year already exists."
                );
            }


            // -----------------------------------------------------
            // Prevent overlapping academic years
            // Exclude current record
            // -----------------------------------------------------
            bool overlappingYear = await _context.AcademicYears
                .AnyAsync(x =>
                    x.Id != model.Id &&
                    model.StartDate <= x.EndDate &&
                    model.EndDate >= x.StartDate);

            if (overlappingYear)
            {
                ModelState.AddModelError(
                    "",
                    "The selected dates overlap with another academic year."
                );
            }


            // -----------------------------------------------------
            // If validation failed
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -----------------------------------------------------
            // Get existing database record
            // -----------------------------------------------------
            var academicYear = await _context.AcademicYears
                .FindAsync(id);

            if (academicYear == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Update only allowed properties
            // -----------------------------------------------------
            academicYear.Name = model.Name;
            academicYear.StartDate = model.StartDate;
            academicYear.EndDate = model.EndDate;
            academicYear.IsActive = model.IsActive;
            academicYear.IsCurrent = model.IsCurrent;


            // -----------------------------------------------------
            // Current year must always be active
            // -----------------------------------------------------
            if (academicYear.IsCurrent)
            {
                academicYear.IsActive = true;

                var currentYears = await _context.AcademicYears
                    .Where(x =>
                        x.IsCurrent &&
                        x.Id != academicYear.Id)
                    .ToListAsync();

                foreach (var year in currentYears)
                {
                    year.IsCurrent = false;
                }
            }


            // -----------------------------------------------------
            // Save changes
            // -----------------------------------------------------
            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Academic year updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AcademicYearExists(id))
                {
                    return NotFound();
                }

                throw;
            }
        }


        // =========================================================
        // GET: /Admin/AcademicYears/Delete/5
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicYear = await _context.AcademicYears
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (academicYear == null)
            {
                return NotFound();
            }

            return View(academicYear);
        }


        // =========================================================
        // POST: /Admin/AcademicYears/Delete/5
        // =========================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var academicYear = await _context.AcademicYears
                .FindAsync(id);

            if (academicYear == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Do not delete current academic year
            // -----------------------------------------------------
            if (academicYear.IsCurrent)
            {
                TempData["Error"] =
                    "The current academic year cannot be deleted.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // Delete
            // -----------------------------------------------------
            _context.AcademicYears.Remove(academicYear);

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Academic year deleted successfully.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "This academic year cannot be deleted because it is already being used by other records.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // POST: /Admin/AcademicYears/SetCurrent/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCurrent(int id)
        {
            var selectedYear = await _context.AcademicYears
                .FirstOrDefaultAsync(x => x.Id == id);

            if (selectedYear == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Only active academic year can become current
            // -----------------------------------------------------
            if (!selectedYear.IsActive)
            {
                TempData["Error"] =
                    "An inactive academic year cannot be set as current.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // Remove current status from all other years
            // -----------------------------------------------------
            var currentYears = await _context.AcademicYears
                .Where(x =>
                    x.IsCurrent &&
                    x.Id != selectedYear.Id)
                .ToListAsync();

            foreach (var year in currentYears)
            {
                year.IsCurrent = false;
            }


            // -----------------------------------------------------
            // Set selected year as current
            // -----------------------------------------------------
            selectedYear.IsCurrent = true;
            selectedYear.IsActive = true;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"{selectedYear.Name} is now the current academic year.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // POST: /Admin/AcademicYears/Deactivate/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var academicYear = await _context.AcademicYears
                .FindAsync(id);

            if (academicYear == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Current year cannot be deactivated
            // -----------------------------------------------------
            if (academicYear.IsCurrent)
            {
                TempData["Error"] =
                    "The current academic year cannot be deactivated.";

                return RedirectToAction(nameof(Index));
            }


            academicYear.IsActive = false;

            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"{academicYear.Name} has been deactivated.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // POST: /Admin/AcademicYears/Activate/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var academicYear = await _context.AcademicYears
                .FindAsync(id);

            if (academicYear == null)
            {
                return NotFound();
            }


            academicYear.IsActive = true;

            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"{academicYear.Name} has been activated.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // Check if academic year exists
        // =========================================================
        private bool AcademicYearExists(int id)
        {
            return _context.AcademicYears
                .Any(e => e.Id == id);
        }
    }
}