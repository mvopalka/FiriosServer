using Firios.Entity;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace FiriosServer.Models.InputModels;

public class UserRegistrationModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string SecondName { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string Position { get; set; } = string.Empty;

    public string? Titules { get; set; } = string.Empty;
    public string? MiddleName { get; set; } = string.Empty;

    public UserEntity ToUserEntity()
    {
        int iteration = new Random().Next(100000, 1000000);

        // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
        byte[] salt = new byte[128 / 8];
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            rngCsp.GetNonZeroBytes(salt);

        }
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: this.Password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: iteration,
            numBytesRequested: 256 / 8));
        return new UserEntity()
        {
            Id = Guid.NewGuid(),
            Titules = this.Titules ?? string.Empty,
            FirstName = this.FirstName,
            MiddleName = this.MiddleName ?? string.Empty,
            SecondName = this.SecondName,
            Email = this.Email,
            PasswordHash = hashed,
            PasswordIteration = iteration,
            PasswordSalt = salt,
            Position = this.Position,
            BrowserData = new List<UserBrowserData>(),
            Incidents = new List<UserIncidentEntity>()
        };
    }
}