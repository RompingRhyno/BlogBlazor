using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogLibrary.Models;

public class Article
{
    [Key]
    public int ArticleId { get; set; }

    [Required]
    public string? Title { get; set; }

    [Column(TypeName = "TEXT")] // Ensure the database column can store large text
    public string? Body { get; set; }

    public DateTime? CreateDate { get; set; }

    [Required]
    public DateTime? StartDate { get; set; }

    [Required]
    public DateTime? EndDate { get; set; }

    public User? Contributor { get; set; }  // Navigation property to User (Contributor)

    // The actual string to store the username
    [EmailAddress]
    [Required]
    public string? ContributorUsername { get; set; }  // Foreign key to User's Username
}
