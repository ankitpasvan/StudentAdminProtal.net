using System.ComponentModel.DataAnnotations;

namespace StudentAdminPortal.Models;


public class SetupViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
