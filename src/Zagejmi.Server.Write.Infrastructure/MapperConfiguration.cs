using AnyMapper;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Infrastructure.Models;

namespace Zagejmi.Server.Write.Infrastructure;

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