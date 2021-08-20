using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Passingwind.LibraryGallery.Domains;

namespace Passingwind.LibraryGallery.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, string>
    {
        public DbSet<Library> Libraries { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Library>(b =>
            {
                b.Property(x => x.Title).IsRequired().HasMaxLength(64);
                b.Property(x => x.Link).HasMaxLength(128);
                b.Property(x => x.UserId).HasMaxLength(64);

                b.HasMany(x => x.Tags).WithOne().HasForeignKey(x => x.LibraryId);
            });

            modelBuilder.Entity<LibraryTags>(b =>
            {
                b.HasKey(x => new { x.LibraryId, x.Name });

                b.Property(x => x.Name).HasMaxLength(32);
            });

        }
    }
}
