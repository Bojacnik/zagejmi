namespace Zagejmi.Domain.Abstractions;

public interface ISoftDelatable
{
    public bool IsSoftDeleted { get; set; }
}