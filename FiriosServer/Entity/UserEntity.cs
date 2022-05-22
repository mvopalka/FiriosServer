using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FiriosServer.Entity.Base;
using Microsoft.EntityFrameworkCore;

namespace FiriosServer.Entity;
[Index(nameof(Email), IsUnique = true)]
public class UserEntity : EntityBase<Guid>
{
    [DisplayName("Tituly")]
    public string Titules { get; set; } = string.Empty;
    [DisplayName("Jméno")]

    [Required] public string FirstName { get; set; } = string.Empty;
    [DisplayName("Prostř. jméno")]

    public string MiddleName { get; set; } = string.Empty;
    [DisplayName("Příjmení")]

    public string SecondName { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required] public string PasswordHash { get; set; } = string.Empty;
    public byte[] PasswordSalt { get; set; }
    public int PasswordIteration { get; set; }
    [DisplayName("Pozice")]

    [Required] public string Position { get; set; } = string.Empty;
    public List<UserBrowserData> BrowserData { get; set; }
    [DisplayName("Výjezdy")]
    public virtual List<UserIncidentEntity> Incidents { get; set; }
}