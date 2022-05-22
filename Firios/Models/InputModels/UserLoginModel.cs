using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Firios.Entity;

namespace Firios.Models.InputModels;

public class UserLoginModel
{
    [Display(Name = "Email Address")]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    [Display(Name = "Heslo")]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public bool IsValidPassword(UserEntity userEntity)
    {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: Password,
            salt: userEntity.PasswordSalt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: userEntity.PasswordIteration,
            numBytesRequested: 256 / 8));
        return userEntity.PasswordHash == hashed;
    }
    public string GenerateSessionId(string uniqueIdentifier)
    {
        TimeSpan objTS = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
        int t = DateTime.Now.Second + DateTime.Now.Millisecond + objTS.Seconds + objTS.Milliseconds;
        long lngRandom = new System.Random(t).Next();
        string strData = uniqueIdentifier + lngRandom.ToString() + RandomNumberGenerator.GetBytes(24);
        string strTmp = "";
        System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
        byte[] hashBytes = encoding.GetBytes(strData);
        SHA512 sha512 = new SHA512CryptoServiceProvider();
        byte[] cryptPassword = sha512.ComputeHash(hashBytes);
        for (int x = 0; x < cryptPassword.Length; x++)
        {
            strTmp = strTmp + String.Format("{0,2:X2}", cryptPassword[x]);
        }
        return strTmp;
    }
}