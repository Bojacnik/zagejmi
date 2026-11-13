namespace Zagejmi.Shared;

/// <summary>
///     Represents an entity that can be softly deleted.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    ///     Gets or sets a value indicating whether the entity is softly deleted.
    /// </summary>
    public bool IsSoftDeleted { get; set; }
}