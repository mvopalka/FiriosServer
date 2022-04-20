namespace Firios.Model;

public class UserPushData
{
    public string Session { get; set; }
    public string Endpoint { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
}