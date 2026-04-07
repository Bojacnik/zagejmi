using System;

namespace Zagejmi.Write.Domain.Abstractions;

/// <summary>
///     Base class for all domain entities.
///     Provides identity-based equality and consistency guarantees.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Entity" /> class with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity. Must not be empty.</param>
    /// <exception>
    ///     Thrown when the provided ID is empty.
    ///     <cref>ArgumentException</cref>
    /// </exception>
    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity ID cannot be empty.", nameof(id));
        }

        this.Id = id;
    }

    /// <summary>
    ///     Gets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns>true if the specified entity is equal to the current entity; otherwise, false.</returns>
    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (this.GetType() != other.GetType())
        {
            return false;
        }

        return this.Id == other.Id;
    }

    /// <summary>
    ///     Determines whether the specified object is equal to the current entity, based on type and unique identifier.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>true if the specified object is equal to the current entity; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Entity other && this.Equals(other);
    }

    /// <summary>
    ///     Returns a hash code for the entity, based on its type and unique identifier.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.GetType(), this.Id);
    }

    /// <summary>
    ///     Determines whether two entities are equal by comparing their types and unique identifiers.
    /// </summary>
    /// <param name="left">Left entity to compare.</param>
    /// <param name="right">Right entity to compare.</param>
    /// <returns>>true if the entities are equal; otherwise, false.</returns>
    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Determines whether two entities are not equal by comparing their types and unique identifiers.
    /// </summary>
    /// <param name="left">Left entity to compare.</param>
    /// <param name="right">Right entity to compare.</param>
    /// <returns>true if the entities are not equal; otherwise, false.</returns>
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }
}