namespace FiriosServer.Models;

public class ServerPushData
{
    public string message { get; set; }
    public string id { get; set; }
    public string smsSentAt { get; set; }
    public string serverReceiveDate { get; set; }
    public string session { get; set; }
}