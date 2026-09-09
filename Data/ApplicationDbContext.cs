using FirstBloom.Models;
using FirstBloom.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<About> Abouts { get; set; }

        public DbSet<Programs> Programs { get; set; }

        public DbSet<Gallery> Galleries { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Testimonial> Testimonials { get; set; }

        public DbSet<SiteSetting> SiteSettings { get; set; }

        public DbSet<ContactMessage> ContactMessages { get; set; }

        public DbSet<FAQ> FAQs { get; set; }

        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<AdmissionApplication> AdmissionApplications {get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Programs>()
                .Property(p => p.Fee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AcademicYear>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<AcademicYear>()
                .HasIndex(a => a.IsCurrent)
                .HasFilter("[IsCurrent] = 1")
                .IsUnique();
        }
    }
}