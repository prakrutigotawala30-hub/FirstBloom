using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Blog
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .OrderByDescending(b => b.PublishedDate)
                .ToListAsync();

            return View(blogs);
        }

        // GET: /Admin/Blog/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.PublishedDate = DateTime.Now;
                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Blog created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(blog);
        }

        // GET: /Admin/Blog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null) return NotFound();

            return View(blog);
        }

        // POST: /Admin/Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Blogs.FindAsync(id);

                    if (existing == null) return NotFound();

                    existing.Title = blog.Title;
                    existing.ShortDescription = blog.ShortDescription;
                    existing.Description = blog.Description;
                    existing.ImageUrl = blog.ImageUrl;
                    existing.Author = blog.Author;
                    existing.Category = blog.Category;
                    existing.ReadTime = blog.ReadTime;
                    existing.IsPublished = blog.IsPublished;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Blog updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Blogs.Any(b => b.Id == blog.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(blog);
        }

        // GET: /Admin/Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null) return NotFound();

            return View(blog);
        }

        // POST: /Admin/Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null) return NotFound();

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Blog deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}