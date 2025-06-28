using System.Transactions;
using AnyMapper;
using Zagejmi.Domain.Community.User;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure;

public class MapperPersonProfile : Profile
{
    public MapperPersonProfile()
    {
        // TODO: test this to check if ReverseMap doesn't fail
        CreateMap<Person, PersonModel>()
            .ForMember(x => x.GoinWalletId, person => person)
            .ForMember(x => x.PersonalInformationId, person => person)
            .ForMember(x => x.StatisticsId, person => person);
    }

    public class MapperGoinTransactionProfile : Profile
    {
        public MapperGoinTransactionProfile()
        {
            // TODO: ignore fields that are unmappable
            CreateMap<Transaction, GoinTransactionModel>();
            //.ReverseMap();
        }
    }
}