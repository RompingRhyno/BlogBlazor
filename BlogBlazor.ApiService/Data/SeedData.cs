using System;
using BlogBlazor.ApiService.Data;
using BlogLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        // Ensure the database is created and migrated
        await context.Database.MigrateAsync();

        // Seed roles
        string[] roleNames = new[] { "Admin", "Contributor" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed Admin user
        var adminEmail =
            Environment.GetEnvironmentVariable("ADMIN_EMAIL")
            ?? throw new InvalidOperationException("ADMIN_EMAIL environment variable is missing.");
        var adminPassword =
            Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
            ?? throw new InvalidOperationException(
                "ADMIN_PASSWORD environment variable is missing."
            );
        var adminFirstName = Environment.GetEnvironmentVariable("ADMIN_FIRSTNAME") ?? "Admin";
        var adminLastName = Environment.GetEnvironmentVariable("ADMIN_LASTNAME") ?? "User";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = adminFirstName,
                LastName = adminLastName,
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Contributor user
        var contributorEmail =
            Environment.GetEnvironmentVariable("CONTRIBUTOR_EMAIL")
            ?? throw new InvalidOperationException(
                "CONTRIBUTOR_EMAIL environment variable is missing."
            );
        var contributorPassword =
            Environment.GetEnvironmentVariable("CONTRIBUTOR_PASSWORD")
            ?? throw new InvalidOperationException(
                "CONTRIBUTOR_PASSWORD environment variable is missing."
            );
        var contributorFirstName =
            Environment.GetEnvironmentVariable("CONTRIBUTOR_FIRSTNAME") ?? "Contributor";
        var contributorLastName =
            Environment.GetEnvironmentVariable("CONTRIBUTOR_LASTNAME") ?? "User";

        var contributorUser = await userManager.FindByEmailAsync(contributorEmail);
        if (contributorUser == null)
        {
            contributorUser = new User
            {
                UserName = contributorEmail,
                Email = contributorEmail,
                FirstName = contributorFirstName,
                LastName = contributorLastName,
            };
            var result = await userManager.CreateAsync(contributorUser, contributorPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(contributorUser, "Contributor");
            }
        }

        // Seed a sample article for the Contributor user
        var existingArticle = await context.Articles.FirstOrDefaultAsync(a =>
            a.Title == "The Rise of AI in Everyday Life: Transforming the Future"
        );
        if (existingArticle == null)
        {
            var article = new Article
            {
                Title = "The Rise of AI in Everyday Life: Transforming the Future",
                Body =
                    @"
                Artificial Intelligence (AI) has rapidly become one of the most transformative technologies of the 21st century. From healthcare to finance, AI is revolutionizing industries, enhancing human capabilities, and reshaping the way we live and work.

                In healthcare, AI algorithms are now able to analyze medical images, predict patient outcomes, and assist doctors in diagnosing diseases with unprecedented accuracy. Machine learning models are being used to develop personalized treatment plans, leading to more effective and efficient care.

                In finance, AI is helping detect fraudulent activities, optimize trading strategies, and provide better customer service through chatbots and virtual assistants. These advancements not only improve the efficiency of financial institutions but also help customers make smarter financial decisions.

                However, the integration of AI into our daily lives raises several ethical questions. While AI holds immense potential, its impact on jobs, privacy, and decision-making must be carefully considered. As AI continues to evolve, it is crucial for developers, policymakers, and society to collaborate in creating frameworks that ensure responsible and transparent use of this powerful technology.

                As we look to the future, the possibilities seem endless. The potential for AI to revolutionize everything from autonomous vehicles to smart cities is just beginning to unfold. It’s clear that AI will continue to shape the world in ways we can only begin to imagine.

                The question is no longer whether AI will change our lives, but how we will adapt to this new, intelligent world.",
                CreateDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7), // End date is 7 days later
                Contributor = contributorUser, // Set the navigation property directly
                ContributorUsername = contributorEmail, // Set the foreign key directly
            };

            await context.Articles.AddAsync(article);
            await context.SaveChangesAsync();
        }
    }
}
