using System.Transactions;
using AutoMapper;
using Zagejmi.Domain.Community.User;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure;

public class MapperPersonProfile : Profile
{
    public MapperPersonProfile()
    {
        // TODO: test this to check if ReverseMap doesn't fail
        CreateMap<Person, PersonModel>()
            .ForMember(x => x.GoinWalletId, opt => opt.Ignore())
            .ForMember(x => x.PersonalInformationId, opt => opt.Ignore())
            .ForMember(x => x.StatisticsId, opt => opt.Ignore())
            .ReverseMap();
    }
}

public class MapperGoinTransactionProfile : Profile
{
    public MapperGoinTransactionProfile()
    {
        // TODO: ignore fields that are unmappable
        CreateMap<Transaction, GoinTransactionModel>()
            .ReverseMap();
    }
}