using System.Net.Mail;
using SharedKernel;
using Zagejmi.Domain.Community.People;

namespace Zagejmi.Domain.Events.People;

public record EventPersonCreated(
    DateTime Timestamp,
    EventTypeDomain EventType,
    Guid AggregateId
)
    : IPersonEvent<Person, Guid>
{
    public required string FirstName { get; init; }
    public required string UserName { get; init; }
    public required string LastName { get; init; }
    public required MailAddress Email { get; init; }
    public required DateTime BirthDate { get; init; }
    public required Gender Gender { get; init; }
    public required PersonType PersonType { get; init; }

    public Person Apply(Person aggregate)
    {
        return new Person(
            AggregateId,
            PersonType,
            new PersonalInformation(Email, UserName, FirstName, LastName, BirthDate, Gender),
            new PersonalStatistics(),
            [],
            associateProfile: null
        );
    }
}