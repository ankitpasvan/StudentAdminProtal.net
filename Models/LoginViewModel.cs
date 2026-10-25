using System.ComponentModel.DataAnnotations;

namespace StudentAdminPortal.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username or email is required.")]
    [StringLength(100, ErrorMessage = "Username or email cannot exceed 100 characters.")]
    [Display(Name = "Username or Email")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
