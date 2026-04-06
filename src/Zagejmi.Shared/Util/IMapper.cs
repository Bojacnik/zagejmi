namespace Zagejmi.Shared.Util;

/// <summary>
///     Defines a contract for mapping entities of one type to another by copying properties with matching names and types.
/// </summary>
public interface IMapper
{
    /// <summary>
    ///     Maps an entity of type TFrom to an entity of type TTo by copying properties with matching names and types.
    /// </summary>
    /// <param name="entity">The source entity to be mapped.</param>
    /// <typeparam name="TFrom">The type of the source entity.</typeparam>
    /// <typeparam name="TTo">The type of the target entity.</typeparam>
    /// <returns>>An instance of TTo with properties copied from the source entity, or null if the source entity is null.</returns>
    TTo? Map<TFrom, TTo>(TFrom entity);
}