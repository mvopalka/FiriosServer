using Firios.Entity.Base;

namespace Firios.Entity;

public class IncidentEntity : EntityBase<Guid>
{
    public string Mpd { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string SubType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string AdditionalInformation { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime Date { get; set; }

    public ICollection<UserIncidentEntity> Users { get; set; } = new List<UserIncidentEntity>();
}