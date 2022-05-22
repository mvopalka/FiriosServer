namespace FiriosServer.Models;

public class UserToIncidentInputModel
{
    public Guid IncidentId { get; set; }
    //public Guid UserId { get; set; }
    public string State { get; set; }
    public string Session { get; set; }
}