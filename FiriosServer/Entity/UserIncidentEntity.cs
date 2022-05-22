using System.ComponentModel.DataAnnotations.Schema;
using FiriosServer.Entity.Base;

namespace FiriosServer.Entity;

public class UserIncidentEntity : EntityBase<Guid>
{
    public Guid IncidentId { get; set; }
    public Guid UserId { get; set; }
    [ForeignKey(nameof(IncidentId))]
    public virtual IncidentEntity IncidentEntity { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual UserEntity UserEntity { get; set; }
    public string State { get; set; }
}