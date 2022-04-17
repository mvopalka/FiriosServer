namespace Firios.Model;

public class UserToIncidentInputModel
{
    public Guid IncidentId { get; set; }
    public Guid UserId { get; set; }
    public string State { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}