using Firios.Entity.Base;
using System.ComponentModel;

namespace Firios.Entity;

public class IncidentEntity : EntityBase<Guid>
{
    public string Mpd { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    [DisplayName("Adresa")]
    public string Address { get; set; } = string.Empty;
    [DisplayName("Podtyp")]

    public string SubType { get; set; } = string.Empty;
    [DisplayName("Typ")]

    public string Type { get; set; } = string.Empty;
    [DisplayName("Objekt")]

    public string ObjectName { get; set; } = string.Empty;
    [DisplayName("Další")]

    public string AdditionalInformation { get; set; } = string.Empty;
    [DisplayName("Stupeň")]

    public string Level { get; set; } = string.Empty;
    [DisplayName("Status")]

    public bool IsActive { get; set; }
    [DisplayName("Datum")]

    public DateTime Date { get; set; }
    [DisplayName("Uživatelé")]
    public List<UserIncidentEntity> Users { get; set; } = new List<UserIncidentEntity>();
}