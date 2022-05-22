namespace Firios.Models.Base;

public interface IModel<Tid>
{
    Tid Id { get; set; }
}