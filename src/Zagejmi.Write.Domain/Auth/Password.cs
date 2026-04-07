using System.Collections.Generic;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Auth;

/// <summary>
///     Represents a user's password in the domain model. This value object encapsulates the securely hashed password along
///     with its associated salt and pepper values, ensuring that password handling adheres to security best practices.
/// </summary>
public record Password : ValueObject
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Password" /> class with the specified hash and salt values. The pepper
    ///     value is typically a secret value stored separately from the database and is not included in this constructor to
    ///     ensure it remains secure.
    /// </summary>
    /// <param name="hash">
    ///     The securely hashed password value, which is the result of hashing the original password combined
    ///     with a unique salt and a secret pepper.
    /// </param>
    /// <param name="salt">
    ///     The unique salt value used in the hashing process to ensure that identical passwords result in
    ///     different hashes, providing protection against rainbow table attacks.
    /// </param>
    /// <param name="pepper">
    ///     The secret pepper value used in the hashing process, which is a fixed secret value stored
    ///     separately from the database and is not included in this constructor to ensure it remains secure. It adds a layer
    ///     of security by making it more difficult for attackers to crack passwords even if they gain access to the database.
    /// </param>
    public Password(string hash, string salt, string pepper)
    {
        this.Hash = hash;
        this.Salt = salt;
        this.Pepper = pepper;
    }

    /// <summary>
    ///     Gets the securely hashed password value. This is the result of hashing the original password combined with a unique
    ///     salt and a secret pepper.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    ///     Gets the unique salt value used in the hashing process. The salt ensures that identical passwords result in
    ///     different hashes, providing protection against rainbow table attacks.
    /// </summary>
    public string Salt { get; }

    /// <summary>
    ///     Gets the secret pepper value used in the hashing process. The pepper is a fixed secret value that is stored
    ///     separately from the database and is not included in the constructor to ensure it remains secure. It adds a layer of
    ///     security by making it more difficult for attackers to crack passwords even if they gain access to the database.
    /// </summary>
    public string Pepper { get; }

    /// <summary>
    ///     Returns an enumeration of the atomic values that define the equality of this value object. In this case, the hash
    ///     and salt values are used to determine equality, while the pepper is intentionally excluded to ensure it remains a
    ///     secret and does not affect equality comparisons.
    /// </summary>
    /// <returns>
    ///     >An enumeration of the atomic values that define the equality of this value object, which includes the hash
    ///     and salt values but excludes the pepper to ensure it remains a secret and does not affect equality comparisons.
    /// </returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Hash;
        yield return this.Salt;
        yield return this.Pepper;
    }
}