using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ganss.Xss;
using BlogServer.Data;
using BlogLibrary.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlogServer.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly AuthenticationStateProvider _authenticationStateProvider;


        public UserService(
            ApplicationDbContext context, 
            UserManager<User> userManager,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _context = context;
            _userManager = userManager;
            _authenticationStateProvider = authenticationStateProvider;
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

        public async Task<string> GetCurrentUsername()
        {
            // Get from authentication state
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.Name ?? string.Empty;
        }
    }
}
