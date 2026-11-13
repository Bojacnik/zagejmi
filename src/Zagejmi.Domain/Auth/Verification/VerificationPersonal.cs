using System;

namespace Zagejmi.Domain.Auth.Verification;

public sealed record VerificationPersonal(Guid Verifier, string Note)
    : Abstractions.Verification
{
    /// <summary>
    ///     Gets the note associated with the verification personal record. This note provides additional context or
    /// </summary>
    public string? Note { get; } = Note;

    /// <summary>
    ///     Gets the unique identifier for the verifier associated with the verification personal record. This identifier is
    ///     used to uniquely identify the verifier and is typically generated when the verification personal record is created.
    /// </summary>
    public Guid Verifier { get; } = Verifier;
}