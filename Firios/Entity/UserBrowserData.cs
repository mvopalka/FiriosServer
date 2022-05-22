using Firios.Entity.Base;

namespace Firios.Entity;

public class UserBrowserData : EntityBase<Guid>
{
    public string Session { get; set; } = string.Empty;
    public DateTime Expire { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public UserEntity UserEntity { get; set; }
}