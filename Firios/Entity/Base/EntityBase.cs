using System.ComponentModel.DataAnnotations;

namespace Firios.Entity.Base;

public abstract class EntityBase<T> : IEntity<T>
{
    [Key] public T Id { get; set; }
}