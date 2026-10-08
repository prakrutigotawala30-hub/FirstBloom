using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Index(
            string? search,
            string? category)
        {
            var query = _context.Galleries.AsQueryable();

            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.Category != null &&
                     x.Category.Contains(search)));
            }

            // -----------------------------------------------------
            // CATEGORY FILTER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x =>
                    x.Category == category);
            }

            // -----------------------------------------------------
            // GET MEDIA
            // -----------------------------------------------------

            var media = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            // -----------------------------------------------------
            // VIEWBAG
            // -----------------------------------------------------

            ViewBag.Search = search;
            ViewBag.Category = category;

            ViewBag.Categories = await _context.Galleries
                .Where(x =>
                    x.Category != null &&
                    x.Category != "")
                .Select(x => x.Category!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return View(media);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Gallery gallery,
            IFormFile? imageFile)
        {
            // -----------------------------------------------------
            // CHECK IMAGE
            // -----------------------------------------------------

            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Please select an image.");
            }

            // -----------------------------------------------------
            // CHECK MODEL
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(gallery);
            }

            // -----------------------------------------------------
            // ALLOWED EXTENSIONS
            // -----------------------------------------------------

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path
                .GetExtension(imageFile!.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

                return View(gallery);
            }

            // -----------------------------------------------------
            // MAX FILE SIZE - 5 MB
            // -----------------------------------------------------

            const long maxFileSize = 5 * 1024 * 1024;

            if (imageFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Image size must be less than 5 MB.");

                return View(gallery);
            }

            // -----------------------------------------------------
            // UPLOAD FOLDER
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
            // UNIQUE FILE NAME
            // -----------------------------------------------------

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                uploadFolder,
                fileName);

            // -----------------------------------------------------
            // SAVE IMAGE
            // -----------------------------------------------------

            try
            {
                await using var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create);

                await imageFile.CopyToAsync(stream);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "imageFile",
                    "The image could not be uploaded. Please try again.");

                return View(gallery);
            }

            // -----------------------------------------------------
            // SAVE IMAGE URL
            // -----------------------------------------------------

            gallery.ImageUrl =
                $"/images/gallery/{fileName}";

            gallery.CreatedAt =
                DateTime.Now;

            // -----------------------------------------------------
            // SAVE DATABASE RECORD
            // -----------------------------------------------------

            _context.Galleries.Add(gallery);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // SUCCESS MESSAGE
            // -----------------------------------------------------

            TempData["Success"] =
                "Image uploaded successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT - GET
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


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Gallery gallery,
            IFormFile? imageFile)
        {
            // -----------------------------------------------------
            // CHECK ID
            // -----------------------------------------------------

            if (id != gallery.Id)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // GET EXISTING RECORD
            // -----------------------------------------------------

            var existing =
                await _context.Galleries
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // CHECK MODEL
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(gallery);
            }

            // -----------------------------------------------------
            // UPDATE BASIC INFORMATION
            // -----------------------------------------------------

            existing.Title =
                gallery.Title;

            existing.Category =
                gallery.Category;

            // -----------------------------------------------------
            // REPLACE IMAGE IF NEW IMAGE SELECTED
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

                var extension = Path
                    .GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

                // -------------------------------------------------
                // CHECK EXTENSION
                // -------------------------------------------------

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "imageFile",
                        "Only JPG, JPEG, PNG and WEBP images are allowed.");

                    return View(gallery);
                }

                // -------------------------------------------------
                // CHECK FILE SIZE
                // -------------------------------------------------

                const long maxFileSize =
                    5 * 1024 * 1024;

                if (imageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "imageFile",
                        "Image size must be less than 5 MB.");

                    return View(gallery);
                }

                // -------------------------------------------------
                // UPLOAD FOLDER
                // -------------------------------------------------

                var uploadFolder = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "gallery");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // -------------------------------------------------
                // DELETE OLD IMAGE
                // -------------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                    existing.ImageUrl))
                {
                    var oldFileName =
                        Path.GetFileName(
                            existing.ImageUrl);

                    var oldFilePath =
                        Path.Combine(
                            uploadFolder,
                            oldFileName);

                    if (System.IO.File.Exists(
                        oldFilePath))
                    {
                        System.IO.File.Delete(
                            oldFilePath);
                    }
                }

                // -------------------------------------------------
                // CREATE NEW FILE NAME
                // -------------------------------------------------

                var fileName =
                    $"{Guid.NewGuid()}{extension}";

                var filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                // -------------------------------------------------
                // SAVE NEW IMAGE
                // -------------------------------------------------

                try
                {
                    await using var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create);

                    await imageFile.CopyToAsync(
                        stream);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(
                        "imageFile",
                        "The image could not be uploaded.");

                    return View(gallery);
                }

                // -------------------------------------------------
                // UPDATE IMAGE URL
                // -------------------------------------------------

                existing.ImageUrl =
                    $"/images/gallery/{fileName}";
            }

            // -----------------------------------------------------
            // SAVE CHANGES
            // -----------------------------------------------------

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Media updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // -----------------------------------------------------
            // FIND RECORD
            // -----------------------------------------------------

            var gallery =
                await _context.Galleries
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (gallery == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // DELETE PHYSICAL IMAGE
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                gallery.ImageUrl))
            {
                var fileName =
                    Path.GetFileName(
                        gallery.ImageUrl);

                var filePath =
                    Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        "gallery",
                        fileName);

                if (System.IO.File.Exists(
                    filePath))
                {
                    System.IO.File.Delete(
                        filePath);
                }
            }

            // -----------------------------------------------------
            // DELETE DATABASE RECORD
            // -----------------------------------------------------

            _context.Galleries.Remove(
                gallery);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            TempData["Success"] =
                "Media deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}