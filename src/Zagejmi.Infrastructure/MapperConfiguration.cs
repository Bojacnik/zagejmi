using AnyMapper;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;
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

        CreateMap<Person, PersonModel>();
        CreateMap<PersonModel, Person>();

        #endregion

        #region GoinWallet Mappings
        
        CreateMap<GoinWallet, GoinWalletModel>();
        CreateMap<GoinWalletModel, GoinWallet>();

        #endregion

        #region GoinTransaction Mappings
        
        CreateMap<GoinTransaction, GoinTransactionModel>();
        CreateMap<GoinTransactionModel, GoinTransaction>();

        #endregion
    }
}