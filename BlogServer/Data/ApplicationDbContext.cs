using BlogLibrary.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogServer.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Article> Articles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Article>().Property(c => c.ArticleId).IsRequired();
        builder.Entity<Article>()
            .HasOne(a => a.Contributor)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Article>().ToTable("Articles");
    }

}
