namespace Zagejmi.Server.Write.Domain.Entity;

public abstract class Entity<TId>(TId id)
    where TId : notnull
{
    public TId Id { get; set; } = id;
}