using Firios.Models.WithoutList;

namespace Firios.Models.WebSocketsModels;

public class WebSocketModel
{
    public string Status { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Position { get; set; }
    public string Action { get; set; }
    public IncidentWithoutList IncidentWithoutList { get; set; }
}