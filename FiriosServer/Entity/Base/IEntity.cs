namespace FiriosServer.Entity.Base;

public interface IEntity<T>
{
    public T Id { get; set; }
}