using Zagejmi.Server.Write.Domain.Auth;
using Zagejmi.Server.Write.Domain.Auth.Hashing;
using Zagejmi.Server.Write.Domain.Community.People;

namespace Zagejmi.Server.Write.Domain.Events.People;

/// <summary>
/// A domain event representing the creation of a new person.
/// This is a simple data contract containing all necessary information to construct the person's initial state.
/// </summary>
public record EventPersonCreated(
    Guid AggregateId,
    string UserName,
    string HashedPassword,
    string Salt,
    HashType HashType,
    string FirstName,
    string LastName,
    string Email,
    DateTime BirthDate,
    Gender Gender,
    PersonType PersonType)
    : IPersonEvent<Person, Guid>
{
    public Person Apply(Person aggregate)
    {
        aggregate.Id = AggregateId;
        aggregate.User = new User(
            UserName,
            new Password(HashedPassword, Salt, HashType),
            Email
        );
        aggregate.PersonalInformation =
            new PersonalInformation(Email, UserName, FirstName, LastName, BirthDate, Gender);
        return aggregate;
    }

    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public EventTypeDomain EventType => EventTypeDomain.PersonCreated;
}