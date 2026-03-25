using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Zagejmi.Write.Domain.Profile;

namespace Zagejmi.Write.Infrastructure.Models;

[Table("Person")]
public class ModelPerson
{
    [Key] public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public PersonType PersonType { get; set; }

    public ModelPersonalInformation PersonalInformation { get; set; }

    public ModelPersonalStatistics PersonalStatistics { get; set; }

    public ModelAssociateProfile? AssociateProfile { get; set; }
}