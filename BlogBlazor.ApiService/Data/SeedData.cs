using System;
using BlogLibrary;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogBlazor.ApiService.Data;

public static class SeedData
{
    public static async Task Initialize(ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        Console.WriteLine("Starting database seeding...");

        // Ensure the database is created and migrated
        Console.WriteLine("Applying migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine("Migrations applied successfully.");

        // Seed roles
        string[] roleNames = new[] { "Admin", "Contributor" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                Console.WriteLine($"Creating role: {roleName}");
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
            else
            {
                Console.WriteLine($"Role already exists: {roleName}");
            }
        }

        // Seed Admin user
        try
        {
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
                ?? throw new InvalidOperationException("ADMIN_EMAIL environment variable is missing.");
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                ?? throw new InvalidOperationException("ADMIN_PASSWORD environment variable is missing.");
            var adminFirstName = Environment.GetEnvironmentVariable("ADMIN_FIRSTNAME") ?? "Admin";
            var adminLastName = Environment.GetEnvironmentVariable("ADMIN_LASTNAME") ?? "User";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                Console.WriteLine($"Creating admin user: {adminEmail}");
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"Admin user {adminEmail} created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors)}");
                }
            }
            else
            {
                Console.WriteLine($"Admin user {adminEmail} already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding admin user: {ex.Message}");
        }

        // Seed Contributor user
        try
        {
            var contributorEmail = Environment.GetEnvironmentVariable("CONTRIBUTOR_EMAIL")
                ?? throw new InvalidOperationException("CONTRIBUTOR_EMAIL environment variable is missing.");
            var contributorPassword = Environment.GetEnvironmentVariable("CONTRIBUTOR_PASSWORD")
                ?? throw new InvalidOperationException("CONTRIBUTOR_PASSWORD environment variable is missing.");
            var contributorFirstName = Environment.GetEnvironmentVariable("CONTRIBUTOR_FIRSTNAME") ?? "Contributor";
            var contributorLastName = Environment.GetEnvironmentVariable("CONTRIBUTOR_LASTNAME") ?? "User";

            var contributorUser = await userManager.FindByEmailAsync(contributorEmail);
            if (contributorUser == null)
            {
                Console.WriteLine($"Creating contributor user: {contributorEmail}");
                contributorUser = new User
                {
                    UserName = contributorEmail,
                    Email = contributorEmail,
                    FirstName = contributorFirstName,
                    LastName = contributorLastName
                };
                var result = await userManager.CreateAsync(contributorUser, contributorPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(contributorUser, "Contributor");
                    Console.WriteLine($"Contributor user {contributorEmail} created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create contributor user: {string.Join(", ", result.Errors)}");
                }
            }
            else
            {
                Console.WriteLine($"Contributor user {contributorEmail} already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding contributor user: {ex.Message}");
        }

        // Seed a sample article for the Contributor user
        try
        {
            Console.WriteLine("Checking if sample article exists...");
            var existingArticle = await context.Articles
                .FirstOrDefaultAsync(a => a.Title == "The Rise of AI in Everyday Life: Transforming the Future");
            if (existingArticle == null)
            {
                Console.WriteLine("Seeding sample article...");
                var article = new Article
                {
                    Title = "The Rise of AI in Everyday Life: Transforming the Future",
                    Body = @"
                    Artificial Intelligence (AI) has rapidly become one of the most transformative technologies of the 21st century...",
                    CreateDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7), // End date is 7 days later
                    ContributorUsername = Environment.GetEnvironmentVariable("CONTRIBUTOR_EMAIL")
                };

                await context.Articles.AddAsync(article);
                await context.SaveChangesAsync();
                Console.WriteLine("Sample article seeded successfully.");
            }
            else
            {
                Console.WriteLine("Sample article already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding article: {ex.Message}");
        }

        Console.WriteLine("Database seeding completed.");
    }
}
