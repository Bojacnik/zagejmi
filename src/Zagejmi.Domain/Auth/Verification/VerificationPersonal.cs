using System;

namespace Zagejmi.Domain.Auth.Verification;

/// <summary>
///     A verification performed in person by another user.
/// </summary>
/// <param name="Verifier">The identifier of the user who performed the verification.</param>
/// <param name="Note">An optional note from the verifier.</param>
public sealed record VerificationPersonal(Guid Verifier, string? Note) : Abstractions.Verification;
