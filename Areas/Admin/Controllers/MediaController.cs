
using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MediaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MediaController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // =========================================================
        // MEDIA LIBRARY
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? category)
        {
            var query = _context.Galleries
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.Category != null &&
                     x.Category.Contains(search)));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x =>
                    x.Category == category);
            }

            var media = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Category = category;

            ViewBag.Categories = await _context.Galleries
                .Where(x => x.Category != null &&
                            x.Category != "")
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return View(media);
        }


        // =========================================================
        // CREATE
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Gallery gallery,
            IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Please select an image.");
            }

            if (!ModelState.IsValid)
            {
                return View(gallery);
            }

            // -----------------------------------------------------
            // Allowed image types
            // -----------------------------------------------------

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension =
                Path.GetExtension(imageFile!.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

                return View(gallery);
            }

            // -----------------------------------------------------
            // Upload folder
            // -----------------------------------------------------

            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "images",
                "gallery");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // -----------------------------------------------------
            // Unique filename
            // -----------------------------------------------------

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath =
                Path.Combine(uploadFolder, fileName);

            // -----------------------------------------------------
            // Save image
            // -----------------------------------------------------

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // -----------------------------------------------------
            // Save database path
            // -----------------------------------------------------

            gallery.ImageUrl =
                $"/images/gallery/{fileName}";

            gallery.CreatedAt = DateTime.Now;

            _context.Galleries.Add(gallery);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Image added to Media Library successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var gallery =
                await _context.Galleries
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (gallery == null)
            {
                return NotFound();
            }

            return View(gallery);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Gallery gallery,
            IFormFile? imageFile)
        {
            if (id != gallery.Id)
            {
                return NotFound();
            }

            var existing =
                await _context.Galleries
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(gallery);
            }

            // -----------------------------------------------------
            // Update basic information
            // -----------------------------------------------------

            existing.Title = gallery.Title;
            existing.Category = gallery.Category;

            // -----------------------------------------------------
            // Replace image if new image selected
            // -----------------------------------------------------

            if (imageFile != null &&
                imageFile.Length > 0)
            {
                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                var extension =
                    Path.GetExtension(imageFile.FileName)
                        .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "imageFile",
                        "Only JPG, JPEG, PNG and WEBP images are allowed.");

                    return View(gallery);
                }

                var uploadFolder = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "gallery");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Delete old image
                if (!string.IsNullOrWhiteSpace(existing.ImageUrl))
                {
                    var oldFileName =
                        Path.GetFileName(existing.ImageUrl);

                    var oldFilePath =
                        Path.Combine(
                            uploadFolder,
                            oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // New filename
                var fileName =
                    $"{Guid.NewGuid()}{extension}";

                var filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                existing.ImageUrl =
                    $"/images/gallery/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Media updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var gallery =
                await _context.Galleries
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (gallery == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Delete physical image
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(gallery.ImageUrl))
            {
                var fileName =
                    Path.GetFileName(gallery.ImageUrl);

                var filePath =
                    Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        "gallery",
                        fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // -----------------------------------------------------
            // Delete database record
            // -----------------------------------------------------

            _context.Galleries.Remove(gallery);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Media deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
