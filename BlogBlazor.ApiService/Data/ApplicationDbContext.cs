using System;
using BlogLibrary;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogBlazor.ApiService.Data;
public class ApplicationDbContext : IdentityDbContext<User> {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Article> Articles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Article>().Property(c => c.ArticleId).IsRequired();      
        builder.Entity<Article>().ToTable("Articles");
    }

    public void Seed(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed data logic here
        SeedData.Initialize(this, userManager, roleManager).Wait();
    }
}
