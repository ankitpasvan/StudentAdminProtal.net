namespace StudentAdminPortal.Models;

// One row per administrator. Passwords are NEVER stored here as plain text:
// PasswordHash holds the output of ASP.NET Core's PasswordHasher<AdminUser>.
public class AdminUser
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
