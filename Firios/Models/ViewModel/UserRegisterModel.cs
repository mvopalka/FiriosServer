using System.ComponentModel.DataAnnotations;

namespace Firios.Models.ViewModel;

public class UserRegisterModel
{
    [Display(Name = "Email Address")]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
    [Required]
    public string Position { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string Surname { get; set; } = string.Empty;
    public string Titules { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;
}