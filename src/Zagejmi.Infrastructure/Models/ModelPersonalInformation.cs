using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Community.People.Person;

namespace Zagejmi.Infrastructure.Models;

[Table("PersonalInformation")]
[Index("Email", IsUnique = true)]
public class ModelPersonalInformation
{
    [Key] public ulong Id { get; init; }
    [StringLength(255)] public required string Email { get; init; }
    [StringLength(255)] public required string UserName { get; init; }

    [StringLength(255)] public required string FirstName { get; init; }
    [StringLength(255)] public required string LastName { get; init; }
    public DateTime BirthDate { get; init; }
    public Gender Gender { get; init; }

    public bool IsVerified { get; init; }

    [ForeignKey("PersonalVerification")] public ulong PersonalVerificationModel { get; init; }
    public ulong? PersonalVerificationId { get; init; }

    [ForeignKey("IdVerification")] public ulong IdVerification { get; init; }
    public ulong? IdVerificationId { get; init; }

    [ForeignKey("FaceVerification")] public ulong FaceVerification { get; init; }
    public ulong? FaceVerificationId { get; init; }
}