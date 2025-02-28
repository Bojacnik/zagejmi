using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Infrastructure.Models.Verification;

[Table("VerificationId")]
public class VerificationIdModel
{
    [Key] public ulong Id { get; set; }

    [Column(TypeName = "varbinary(max)")] public required byte[] ImageFront { get; set; }

    [Column(TypeName = "varbinary(max)")] public required byte[] ImageBack { get; set; }
}