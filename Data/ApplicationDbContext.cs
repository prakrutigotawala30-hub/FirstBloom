using Microsoft.EntityFrameworkCore;
using FirstBloom.Models;

namespace FirstBloom.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<About> Abouts { get; set; }
        public DbSet<Programs> Programs { get; set; }

        public DbSet<Gallery> Galleries { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Testimonial> Testimonials { get; set; }

        public DbSet<SiteSetting> SiteSettings { get; set; }
    }
}