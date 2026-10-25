using System.ComponentModel.DataAnnotations;

namespace StudentAdminPortal.Models;

public class Student
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course is required.")]
    [StringLength(100, ErrorMessage = "Course cannot exceed 100 characters.")]
    public string Course { get; set; } = string.Empty;

    // Set by the database (DEFAULT SYSUTCDATETIME()), never entered in a form.
    [Display(Name = "Registration Date")]
    public DateTime RegistrationDate { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
