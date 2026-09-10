using FirstBloom.Data;
using FirstBloom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Blog
        public async Task<IActionResult> Index(string category, string search)
        {
            var blogs = _context.Blogs
                .Where(b => b.IsPublished)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                blogs = blogs.Where(b => b.Category == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                blogs = blogs.Where(b =>
                    b.Title.Contains(search) ||
                    b.ShortDescription.Contains(search));
            }

            var result = await blogs
                .OrderByDescending(b => b.PublishedDate)
                .ToListAsync();

            var popular = await _context.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedDate)
                .Take(3)
                .ToListAsync();

            ViewBag.Categories = await _context.Blogs
                .Where(b => b.IsPublished)
                .Select(b => b.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.PopularPosts = popular;
            ViewBag.SelectedCategory = category ?? "All";
            ViewBag.Search = search;

            return View(result);
        }

        // GET: /Blog/Details/1
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && b.IsPublished);

            if (blog == null)
            {
                return NotFound();
            }

            var related = await _context.Blogs
                .Where(b => b.IsPublished &&
                       b.Category == blog.Category &&
                       b.Id != blog.Id)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedPosts = related;

            return View(blog);
        }
    }
}