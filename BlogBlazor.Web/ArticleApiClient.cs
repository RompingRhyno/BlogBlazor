using System.Net.Http.Json;
using BlogLibrary.Models;

namespace BlogBlazor.Web;

public class ArticleApiClient(HttpClient httpClient)
{
    public async Task<Article[]> GetArticlesAsync(
        int maxItems = 10,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var articles = await httpClient.GetFromJsonAsync<Article[]>(
                "/api/articles",
                cancellationToken
            );
            return articles?.Take(maxItems).ToArray() ?? [];
        }
        catch (HttpRequestException ex)
        {
            // Log the error or handle it as needed
            Console.WriteLine($"Error fetching articles: {ex.Message}");
            return [];
        }
    }

    public async Task<Article?> GetArticleByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await httpClient.GetFromJsonAsync<Article>(
                $"/api/articles/{id}",
                cancellationToken
            );
        }
        catch (HttpRequestException ex)
        {
            // Log the error or handle it as needed
            Console.WriteLine($"Error fetching article with ID {id}: {ex.Message}");
            return null;
        }
    }
}
