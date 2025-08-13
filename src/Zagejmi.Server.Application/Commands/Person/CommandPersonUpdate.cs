using System.Net.Mail;

namespace Zagejmi.Server.Application.Commands.Person;

public record CommandPersonUpdate(
    
    // Personal Info
    Guid OldPersonId,
    Guid NewPersonId,
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender,
    PersonType PersonType,
    
    // Personal Stats
    ulong TotalScore,
    ulong Level,
    ulong TimeTotal,
    ulong TimeWatching,
    uint ChatsSent,
    ulong CzkSpent,
    ulong GoingSpent,
    ulong TransactionsAmount,
    ulong GiftsSent
    
) : ICommandPerson;

public enum PersonType
{
    Anon,
    Customer,
    VerifiedCustomer,
    Associate,
    VerifiedAssociate,
    Admin
}