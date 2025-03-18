using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Infrastructure.Models.Verification;

[Table("Verification")]
public class VerificationModel
{
    [Key] public ulong Id { get; init; }

    [ForeignKey("VerificationPersonal")] public VerificationPersonalModel? VerificationPersonal { get; init; }
    public ulong? VerificationPersonalId { get; init; }

    [ForeignKey("VerificationId")] public VerificationIdModel? VerificationId { get; init; }
    public ulong? VerificationIdId { get; init; }

    [ForeignKey("VerificationFace")] public VerificationFaceModel? VerificationFace { get; init; }
    public ulong? VerificationFaceId { get; init; }
}