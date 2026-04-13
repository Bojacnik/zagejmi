namespace Zagejmi.Read.Api.Models;

/// <summary>
///     Supported user profile categories in the read model.
/// </summary>
public enum ProfileType
{
    /// <summary>
    ///     Regular customer profile.
    /// </summary>
    Customer,

    /// <summary>
    ///     Associate profile.
    /// </summary>
    Associate,

    /// <summary>
    ///     Anonymous profile.
    /// </summary>
    Anon
}