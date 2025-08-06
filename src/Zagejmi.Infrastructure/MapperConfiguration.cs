using AnyMapper;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Community.People.Person;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure;

/// <summary>
/// A consolidated mapping profile for all infrastructure-level mappings.
/// This approach centralizes mapping configuration, making it easier to manage and register
/// in the application's dependency injection container.
/// </summary>
public class InfrastructureMappingProfile : Profile
{
    public InfrastructureMappingProfile()
    {
        #region Person Mappings

        CreateMap<Person, ModelPerson>();
        CreateMap<ModelPerson, Person>();

        #endregion

        #region GoinWallet Mappings

        CreateMap<GoinWallet, ModelGoinWallet>();
        CreateMap<ModelGoinWallet, GoinWallet>();

        #endregion

        #region GoinTransaction Mappings

        CreateMap<GoinTransaction, ModelGoinTransaction>();
        CreateMap<ModelGoinTransaction, GoinTransaction>();

        #endregion
    }
}