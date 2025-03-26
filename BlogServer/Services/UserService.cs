using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ganss.Xss;
using BlogServer.Data;
using BlogLibrary.Models;

namespace BlogServer.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
    
        public async Task<List<Article>> GetAllArticlesAsync()
        {
            var currentDate = DateTime.UtcNow;

            return await _context.Articles
                .Include(a => a.Contributor)  // Eager load the Contributor navigation property
                .Where(a => a.StartDate <= currentDate && a.EndDate >= currentDate)
                .OrderByDescending(a => a.CreateDate)
                .ToListAsync();
        }

        public async Task<Article?> GetArticleDetailsAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Contributor)  // Eagerly load
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<bool> CreateArticleAsync(
            Article article, 
            string currentUsername)
        {
            if (article.StartDate.HasValue && article.EndDate.HasValue && 
                    article.EndDate < article.StartDate)
                {
                    return false;
                }

                var sanitizer = new HtmlSanitizer();
                article.Body = sanitizer.Sanitize(article.Body ?? string.Empty);
                
                var user = await _userManager.FindByNameAsync(currentUsername);
                if (user == null) return false;

                article.Contributor = user;
                article.ContributorUsername = user.UserName!;
                article.CreateDate = DateTime.UtcNow;

                await _context.Articles.AddAsync(article);
                await _context.SaveChangesAsync();

                return true;
        }

        public async Task<bool> CanEditArticleAsync(int articleId, string currentUsername, bool isAdmin)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
            
            if (article == null) return false;
            
            return article.ContributorUsername == currentUsername || isAdmin;
        }

        public async Task<bool> EditArticleAsync(int id, Article updatedArticle, string currentUsername)
        {
            var existingArticle = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            
            if (existingArticle == null) return false;
            
            if (updatedArticle.StartDate.HasValue && updatedArticle.EndDate.HasValue && 
                updatedArticle.EndDate < updatedArticle.StartDate)
            {
                return false;
            }

            var sanitizer = new HtmlSanitizer();
            updatedArticle.Body = sanitizer.Sanitize(updatedArticle.Body ?? string.Empty);

            existingArticle.Title = updatedArticle.Title;
            existingArticle.Body = updatedArticle.Body;
            existingArticle.StartDate = updatedArticle.StartDate;
            existingArticle.EndDate = updatedArticle.EndDate;
            existingArticle.ContributorUsername = currentUsername;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return false;

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

    // [HttpPost]
    // [Authorize(Roles = "Contributor,Admin")]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> Create(Article article)
    // {   
    //     if (User.Identity == null)
    //     {
    //         return Forbid();
    //     }
    //     article.ContributorUsername = User.Identity.Name;
    //     ModelState.Remove("ContributorUsername");  // Remove from validation if necessary
        
    //     if (ModelState.IsValid)
    //     {
    //         if (article.StartDate.HasValue && article.EndDate.HasValue && article.EndDate < article.StartDate)
    //         {
    //             ModelState.AddModelError("EndDate", "End Date must be after Start Date.");
    //             return View(article);
    //         }

    //         // Sanitize the article Body content to prevent XSS
    //         var sanitizer = new HtmlSanitizer();
    //         article.Body = sanitizer.Sanitize(article.Body ?? string.Empty);  // Fallback to empty string if Body is null
    //         var user = await _userManager.GetUserAsync(User);

    //         if (user == null)
    //         {
    //             return Forbid();
    //         }

    //         article.Contributor = user;
    //         article.ContributorUsername = user.UserName!;
    //         article.CreateDate = DateTime.UtcNow;

    //         _context.Articles.Add(article);
    //         await _context.SaveChangesAsync();

    //         return RedirectToAction(nameof(Index));
    //     }
    //     else
    //     {
    //         // Print out the model errors to see what might be invalid
    //         foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
    //         {
    //             Console.WriteLine($"Error: {error.ErrorMessage}");
    //         }
    //     }

    //     return View(article);
    // }

//     [HttpGet]
//     [Authorize(Roles = "Contributor,Admin")]
//     public async Task<IActionResult> Edit(int id)
//     {
//         var article = await _context.Articles.FindAsync(id);

//         if (article == null)
//         {
//             return NotFound();
//         }

//         // Check if the user identity is null
//         if (User.Identity?.Name == null)
//         {
//             return Forbid();
//         }

//         // Ensure the user is either the contributor or an admin
//         if (article.ContributorUsername != User.Identity.Name && !User.IsInRole("Admin"))
//         {
//             var user = await _userManager.GetUserAsync(User);
//             var username = user?.UserName;
//             var contributorUsername = article.ContributorUsername;
//             Console.WriteLine(username);
//             Console.WriteLine(contributorUsername);
//             Console.WriteLine("User is not the contributor or an admin");
//             return Forbid();
//         }

//         return View(article);
//     }

//     [HttpPost]
//     [Authorize(Roles = "Contributor,Admin")]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> Edit(int id, Article article)
//     {
//         if (id != article.ArticleId)
//         {
//             return NotFound();
//         }

//         var existingArticle = await _context.Articles
//             .Include(a => a.Contributor)  // Eagerly load
//             .FirstOrDefaultAsync(a => a.ArticleId == id);

//         // Check if the article exists
//         if (existingArticle == null)
//         {
//             return NotFound();
//         }

//         // Check if the user identity is null
//         if (User.Identity?.Name == null)
//         {
//             return Forbid();
//         }

//         // Ensure the user is either the contributor or an admin
//         if (existingArticle.Contributor?.UserName != User.Identity.Name && !User.IsInRole("Admin"))
//         {
//             return Forbid();
//         }

//         // Automatically set ContributorUsername to the logged-in user's name
//         article.ContributorUsername = User.Identity.Name;  // Set on the passed article object
//         Console.WriteLine(article.ContributorUsername);
//         ModelState.Remove("ContributorUsername");  // Remove the ContributorUsername from the ModelState

//         if (ModelState.IsValid)
//         {
//             // Validation for dates
//             if (article.StartDate.HasValue && article.EndDate.HasValue && article.EndDate < article.StartDate)
//             {
//                 ModelState.AddModelError("EndDate", "End Date must be after Start Date.");
//                 return View(article);
//             }

//             // Sanitize the article Body content to prevent XSS
//             var sanitizer = new HtmlSanitizer();
//             article.Body = sanitizer.Sanitize(article.Body ?? string.Empty);  // Fallback to empty string if Body is null

//             // Update article properties
//             existingArticle.Title = article.Title;
//             existingArticle.Body = article.Body;
//             existingArticle.StartDate = article.StartDate;
//             existingArticle.EndDate = article.EndDate;

//             // Save changes to database
//             _context.Update(existingArticle);
//             await _context.SaveChangesAsync();

//             return RedirectToAction(nameof(Index));
//         }
//         return View(article);
//     }

//     [HttpPost]
//     [Authorize(Roles = "Contributor,Admin")]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> Delete(int id)
//     {
//         var article = await _context.Articles.FindAsync(id);
//         if (article == null)
//         {
//             return NotFound();
//         }

//         _context.Articles.Remove(article);
//         await _context.SaveChangesAsync();

//         return RedirectToAction(nameof(Index));
//     }

//     public IActionResult Privacy()
//     {
//         return View();
//     }
// }
