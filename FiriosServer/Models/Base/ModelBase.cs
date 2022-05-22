namespace FiriosServer.Models.Base;

public class ModelBase : IModel<Guid>
{
    public Guid Id { get; set; }
}