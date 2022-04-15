using Firios.Entity.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Firios.Entity;
[Index(nameof(Email), IsUnique = true)]
public class UserEntity : EntityBase<Guid>
{
    // TODO: Add atributes for password sald and interation count
    public string Titules { get; set; } = string.Empty;

    [Required] public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required] public string PasswordHash { get; set; } = string.Empty;
    public byte[] PasswordSalt { get; set; }
    public int PasswordIteration { get; set; }

    [Required] public string Position { get; set; } = string.Empty;
    public List<UserBrowserData> BrowserData { get; set; }

    public virtual List<UserIncidentEntity> Incidents { get; set; }
}