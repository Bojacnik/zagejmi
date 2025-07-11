﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Infrastructure.Models;

[Table("Person")]
public class ModelPerson
{
    [Key] public ulong Id { get; init; }

    public Guid DomainId { get; init; }

    public PersonType PersonType { get; init; }

    public ModelPersonalInformation ModelPersonalInfo { get; init; }

    public ModelPersonalStatistics ModelPersonalStatistics { get; init; }

    public ModelAssociateProfile? AssociateProfileModel { get; init; }

    public ModelPerson(
        ulong id,
        PersonType personType,
        ModelPersonalInformation modelPersonalInfo,
        ModelPersonalStatistics modelPersonalStatistics)
    {
        Id = id;
        PersonType = personType;
        ModelPersonalInfo = modelPersonalInfo;
        ModelPersonalStatistics = modelPersonalStatistics;
    }
}