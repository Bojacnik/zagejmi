using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Infrastructure.Models.Verification;

[Table("VerificationPersonal")]
public abstract class VerificationPersonalModel
{
    [Key] public ulong Id { get; init; }

    [ForeignKey("Person")] public PersonModel? Verifier { get; init; }
    public ulong? VerifierId { get; init; }

    public string? Note { get; init; }
}