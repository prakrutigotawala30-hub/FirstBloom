using FirstBloom.Models;
using FirstBloom.Models.Identity;
using FirstBloom.Models.Student;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstBloom.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        // =====================================================
        // WEBSITE
        // =====================================================

        public DbSet<About> Abouts { get; set; }

        public DbSet<Programs> Programs { get; set; }

        public DbSet<Gallery> Galleries { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Testimonial> Testimonials { get; set; }

        public DbSet<SiteSetting> SiteSettings { get; set; }

        public DbSet<ContactMessage> ContactMessages { get; set; }

        public DbSet<FAQ> FAQs { get; set; }


        // =====================================================
        // NOTICE
        // =====================================================

        public DbSet<Notice> Notices { get; set; }

        public DbSet<NoticeRead> NoticeReads { get; set; }


        // =====================================================
        // ACADEMIC YEAR
        // =====================================================

        public DbSet<AcademicYear> AcademicYears { get; set; }


        // =====================================================
        // ADMISSION
        // =====================================================

        public DbSet<AdmissionApplication>
            AdmissionApplications
        { get; set; }


        // =====================================================
        // STUDENT
        // =====================================================

        public DbSet<StudentProfile>
            StudentProfiles
        { get; set; }


        // =====================================================
        // MODEL CONFIGURATION
        // =====================================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =================================================
            // PROGRAM FEE
            // =================================================

            modelBuilder.Entity<Programs>()
                .Property(p => p.Fee)
                .HasPrecision(18, 2);


            // =================================================
            // ACADEMIC YEAR NAME
            // =================================================

            modelBuilder.Entity<AcademicYear>()
                .HasIndex(a => a.Name)
                .IsUnique();


            // =================================================
            // ONLY ONE CURRENT ACADEMIC YEAR
            // =================================================

            modelBuilder.Entity<AcademicYear>()
                .HasIndex(a => a.IsCurrent)
                .HasFilter("[IsCurrent] = 1")
                .IsUnique();


            // =================================================
            // NOTICE READ
            // =================================================

            modelBuilder.Entity<NoticeRead>()
                .HasIndex(x => new
                {
                    x.NoticeId,
                    x.UserId
                })
                .IsUnique();
        }
    }
}