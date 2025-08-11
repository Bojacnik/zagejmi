using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zagejmi.Server.Write.Domain.Community.People;

namespace Zagejmi.Server.Write.Infrastructure.Models;

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