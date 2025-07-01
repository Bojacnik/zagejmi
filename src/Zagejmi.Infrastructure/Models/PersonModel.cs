using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Infrastructure.Models;

[Table("Person")]
public class PersonModel
{
    [Key] public ulong Id { get; init; }

    public PersonType PersonType { get; init; }

    public PersonalInformationModel PersonalInfo { get; init; }

    public PersonalStatisticsModel PersonalStatistics { get; init; }
    
    public AssociateProfileModel? AssociateProfileModel { get; init; }

    public PersonModel(
        ulong id,
        PersonType personType,
        PersonalInformationModel personalInfo,
        PersonalStatisticsModel personalStatistics)
    {
        Id = id;
        PersonType = personType;
        PersonalInfo = personalInfo;
        PersonalStatistics = personalStatistics;
    }
}