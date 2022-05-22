using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Firios.Entity;

namespace Firios.Models.InputModels;

public class UserChangePasswordModel
{
    [DisplayName("Staré heslo")]
    [DataType(DataType.Password)]
    public string PasswordOld { get; set; } = string.Empty;
    [DisplayName("Heslo")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [DisplayName("Ověření hesla")]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }


    public UserEntity ToUserEntity(UserEntity userEntity)
    {
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
    public bool IsValidPassword(UserEntity userEntity)
    {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: PasswordOld,
            salt: userEntity.PasswordSalt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: userEntity.PasswordIteration,
            numBytesRequested: 256 / 8));
        return userEntity.PasswordHash == hashed;
    }
}