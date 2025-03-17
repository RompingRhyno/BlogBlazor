using BlogBlazor.ApiService.Data;
using BlogLibrary.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Env.Load(); // This loads the .env file
var builder = WebApplication.CreateBuilder(args);

builder.AddSqlServerDbContext<ApplicationDbContext>("sqldata");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add Cors
builder.Services.AddCors(o =>
    o.AddPolicy(
        "Policy",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    )
);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Policy");

// Endpoint to retrieve all articles (GET)
app.MapGet("/api/articles", async (ApplicationDbContext db) => await db.Articles.ToListAsync());

// Endpoint to retrieve a specific article by id (GET)
app.MapGet(
    "/api/articles/{id}",
    async (int id, ApplicationDbContext db) =>
        await db.Articles.FindAsync(id) is Article article
            ? Results.Ok(article)
            : Results.NotFound()
);

// // Endpoint to create a new article (POST)
// app.MapPost(
//     "/api/articles",
//     async (Article article, ApplicationDbContext db) =>
//     {
//         db.Articles.Add(article);
//         await db.SaveChangesAsync();
//         return Results.Created($"/api/articles/{article.ArticleId}", article);
//     }
// );

// // Endpoint to update an existing article (PUT)
// app.MapPut(
//     "/api/articles/{id}",
//     async (int id, Article updatedArticle, ApplicationDbContext db) =>
//     {
//         var article = await db.Articles.FindAsync(id);
//         if (article is null)
//             return Results.NotFound();

//         article.Title = updatedArticle.Title;
//         article.Body = updatedArticle.Body;
//         article.StartDate = updatedArticle.StartDate;
//         article.EndDate = updatedArticle.EndDate;
//         article.ContributorUsername = updatedArticle.ContributorUsername;

//         await db.SaveChangesAsync();
//         return Results.Ok(article);
//     }
// );

// // Endpoint to delete an article (DELETE)
// app.MapDelete(
//     "/api/articles/{id}",
//     async (int id, ApplicationDbContext db) =>
//     {
//         var article = await db.Articles.FindAsync(id);
//         if (article is null)
//             return Results.NotFound();

//         db.Articles.Remove(article);
//         await db.SaveChangesAsync();
//         return Results.Ok();
//     }
// );

app.UseAuthorization();
app.MapControllers();

// Migrate & Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.Migrate();
        await SeedData.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
