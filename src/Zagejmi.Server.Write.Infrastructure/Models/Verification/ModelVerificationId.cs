using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Server.Write.Infrastructure.Models.Verification;

[Table("VerificationId")]
public class ModelVerificationId
{
    [Key] public ulong Id { get; set; }
    
    public Guid DomainId { get; init; }

    [Column(TypeName = "varbinary(max)")] public required byte[] ImageFront { get; set; }

    [Column(TypeName = "varbinary(max)")] public required byte[] ImageBack { get; set; }
}