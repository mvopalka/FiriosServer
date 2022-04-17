namespace Firios.Model.Base;

public interface IModel<Tid>
{
    Tid Id { get; set; }
}