using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Firios.Entity;

namespace Firios.Models.InputModels;

public class UserEditModel
{
    public Guid Id;
    [DisplayName("Jméno")]
    public string? FirstName { get; set; } = string.Empty;
    [DisplayName("Příjmení")]
    public string? SecondName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    [DisplayName("Heslo")]
    [DataType(DataType.Password)]
    public string? Password { get; set; } = string.Empty;
    [DisplayName("Ověření hesla")]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string? ConfirmPassword { get; set; }
    [DisplayName("Pozice")]
    public string? Position { get; set; } = string.Empty;
    [DisplayName("Tituly")]
    public string? Titules { get; set; } = string.Empty;
    [DisplayName("Prostř. jméno")]
    public string? MiddleName { get; set; } = string.Empty;

    public UserEntity ToUserEntity(UserEntity userEntity)
    {
        userEntity.FirstName = string.IsNullOrEmpty(FirstName) ? userEntity.FirstName : FirstName;
        userEntity.SecondName = string.IsNullOrEmpty(SecondName) ? userEntity.SecondName : SecondName;
        userEntity.Position = string.IsNullOrEmpty(Position) ? userEntity.Position : Position;
        userEntity.Titules = Titules ?? string.Empty;
        userEntity.MiddleName = MiddleName ?? string.Empty;
        userEntity.Email = string.IsNullOrEmpty(Email) ? userEntity.Email : Email;

        if (!string.IsNullOrEmpty(this.Password))
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: this.Password,
                salt: userEntity.PasswordSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: userEntity.PasswordIteration,
                numBytesRequested: 256 / 8));
            if (hashed != userEntity.PasswordHash)
            {
                int iteration = new Random().Next(100000, 1000000);

                // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                byte[] salt = new byte[128 / 8];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);

                }
                hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: this.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: iteration,
                    numBytesRequested: 256 / 8));

                userEntity.PasswordSalt = salt;
                userEntity.PasswordIteration = iteration;
                userEntity.PasswordHash = hashed;
            }
        }

        return userEntity;
    }

    public static UserEditModel From(UserEntity userEntity)
    {
        return new UserEditModel()
        {
            Id = userEntity.Id,
            Email = userEntity.Email,
            FirstName = userEntity.FirstName,
            SecondName = userEntity.SecondName,
            Position = userEntity.Position,
            MiddleName = userEntity.MiddleName,
            Titules = userEntity.Titules,
            Password = string.Empty
        };
    }
}