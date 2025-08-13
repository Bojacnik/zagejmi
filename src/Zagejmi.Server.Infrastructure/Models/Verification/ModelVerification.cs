using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Server.Infrastructure.Models.Verification;

[Table("Verification")]
public class ModelVerification
{
    [Key] public ulong Id { get; init; }

    [ForeignKey("VerificationPersonal")] public ModelVerificationPersonal? VerificationPersonal { get; init; }
    public ulong? VerificationPersonalId { get; init; }

    [ForeignKey("VerificationId")] public ModelVerificationId? VerificationId { get; init; }
    public ulong? VerificationIdId { get; init; }

    [ForeignKey("VerificationFace")] public ModelVerificationFace? VerificationFace { get; init; }
    public ulong? VerificationFaceId { get; init; }
}