using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Server.Write.Infrastructure.Models.Verification;

[Table("VerificationPersonal")]
public abstract class ModelVerificationPersonal
{
    [Key] public ulong Id { get; init; }
    
    public Guid DomainId { get; init; }

    [ForeignKey("Person")] public ModelPerson? Verifier { get; init; }
    public ulong? VerifierId { get; init; }

    public string? Note { get; init; }
}