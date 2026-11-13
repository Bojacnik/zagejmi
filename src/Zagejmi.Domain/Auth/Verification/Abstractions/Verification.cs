namespace Zagejmi.Domain.Auth.Verification.Abstractions;

/// <summary>
///     Represents the base class for verification records in the authentication system. This abstract record defines
///     common properties and behaviors for different types of verification records, such as verification of identification
///     documents, personal information, or other verification processes. Derived classes should implement specific
///     verification types and provide additional properties and methods as needed to support the verification process. The
///     Verification base class ensures consistency and reusability across different verification implementations, allowing
///     for a unified approach to managing user verification within the authentication system. It is important to handle
///     verification records securely and protect sensitive information to maintain the integrity and trustworthiness of
///     the verification process.
/// </summary>
public abstract record Verification
{
    /// <summary>
    ///     Gets the type of verification represented by this record. The Type property indicates the specific verification
    ///     process or category associated with the verification record, allowing for differentiation between various
    ///     verification types (e.g., identification documents, personal information, etc.). This property is essential for
    ///     managing and processing verification records appropriately based on their type, ensuring that the correct
    ///     validation and handling procedures are applied to each verification record. Derived classes should set the Type
    ///     property to the appropriate VerificationType value to indicate the specific verification process they represent.
    /// </summary>
    public VerificationType Type { get; protected init; }

    /// <summary>
    ///     Gets or sets a value indicating whether the verification process has been successfully completed and verified. The
    ///     Verified
    ///     property reflects the current status of the verification record, allowing for easy tracking and management of
    ///     verification processes. A value of true indicates that the verification has been successfully completed and
    ///     validated, while a value of false indicates that the verification is still pending or has not been successfully
    ///     completed. This property is essential for determining the outcome of the verification process and for making
    ///     decisions based on the verification status of the record. Derived classes should update the Verified property as
    ///     appropriate based on the results of the verification process, ensuring that the status accurately reflects the
    ///     current state of the verification record.
    /// </summary>
    public bool Verified { get; protected set; }
}
