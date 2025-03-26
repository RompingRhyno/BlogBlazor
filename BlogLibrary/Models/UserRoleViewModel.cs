namespace BlogLibrary.Models
{
    public class UserRoleViewModel
    {
        public string? UserName { get; set; }
        public IList<string>? Roles { get; set; }
    }
}