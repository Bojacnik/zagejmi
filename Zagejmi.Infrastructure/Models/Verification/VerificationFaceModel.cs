using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Infrastructure.Models.Verification;

[Table("VerificationFace")]
public class VerificationFaceModel
{
    [Key] public ulong Id { get; set; }

    [StringLength(512)] public required string Path { get; set; }
    [Column(TypeName = "varbinary(max)")] public required byte[] ImageFront { get; set; }
}