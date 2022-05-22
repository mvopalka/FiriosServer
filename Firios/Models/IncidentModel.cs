using Firios.Models.Base;

namespace Firios.Models;

public class IncidentModel : ModelBase
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

    public ICollection<User> Users { get; set; } = new List<User>();

    public class User : ModelBase
    {
        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;

    }
}