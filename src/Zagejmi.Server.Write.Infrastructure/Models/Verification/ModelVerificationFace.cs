using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Server.Write.Infrastructure.Models.Verification;

[Table("VerificationFace")]
public class ModelVerificationFace
{
    [Key] public ulong Id { get; set; }
    
    public Guid DomainId { get; init; }

    [StringLength(512)] public required string Path { get; set; }
    [Column(TypeName = "varbinary(max)")] public required byte[] ImageFront { get; set; }
}