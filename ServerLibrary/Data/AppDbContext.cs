using BaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.Title)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(b => b.Author)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(b => b.Genre)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(b => b.PublishedDate)
                .IsRequired();
        });
    }
}
