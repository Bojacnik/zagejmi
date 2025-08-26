using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Zagejmi.Server.Write.Domain.Community.People;

namespace Zagejmi.Server.Infrastructure.Models;

[Table("PersonalInformation")]
[Index("MailAddress", IsUnique = true)]
public class ModelPersonalInformation
{
    [Key] public ulong Id { get; set; }
    [StringLength(255)] public string? MailAddress { get; set; }
    [StringLength(255)] public string? UserName { get; set; }

    [StringLength(255)] public string? FirstName { get; set; }
    [StringLength(255)] public string? LastName { get; set; }
    public DateTime BirthDay { get; set; }
    public Gender Gender { get; set; }

    public bool IsVerified { get; set; }

    public ModelPersonalInformation() {}
}
