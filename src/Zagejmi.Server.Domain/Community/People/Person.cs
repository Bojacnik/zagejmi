using LanguageExt;
using System;
using System.Collections.Generic;
using Zagejmi.Server.Domain.Community.Goin;
using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;
using Zagejmi.Server.Domain.Events.Auth;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.People.Associate;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain.Community.People;

public sealed class Person : Aggregate<Person, Guid>
{
    #region Person Properties

    public Guid UserId { get; private set; }
    public PersonType PersonType { get; private set; }
    public PersonalInformation PersonalInformation { get; private set; }
    public PersonalStatistics PersonalStatistics { get; private set; }
    public List<GoinWallet> Wallets { get; private set; } = new();
    public AssociateProfile? AssociateProfile { get; private set; }
    public bool Deleted { get; private set; }

    #endregion

    // Private constructor for rehydration and creation
    private Person(Guid id) : base(id) { }

    public static Either<Failure, Person> Create(
        Guid personId,
        Guid userId,
        string email,
        string userName,
        string firstName,
        string lastName,
        DateTime birthDate,
        Write.Domain.Community.People.Gender gender)
    {
        if (userId == Guid.Empty)
        {
            return new FailureArgumentInvalidValue("A person must be associated with a user.");
        }

        Person person = new Person(personId);
        PersonalInformation personalInfo = new PersonalInformation(email, userName, firstName, lastName, birthDate, gender);
        
        person.RaiseEvent(new EventPersonCreated(
            personId,
            userId,
            PersonType.Customer, // Assuming Customer is the default
            personalInfo,
            new PersonalStatistics(), // Assuming stats start empty
            new List<GoinWallet>(), // Assuming wallets start empty
            null // Assuming no associate profile at creation
        ));

        return person;
    }

    protected override void Apply(IDomainEvent<Person, Guid> evt)
    {
        switch (evt)
        {
            case EventPersonCreated e:
                OnPersonCreated(e);
                break;
        }
    }

    private void OnPersonCreated(EventPersonCreated e)
    {
        Id = e.PersonId;
        UserId = e.UserId;
        PersonType = e.PersonType;
        PersonalInformation = e.PersonalInformation;
        PersonalStatistics = e.PersonalStatistics;
        Wallets = e.Wallets;
        AssociateProfile = e.AssociateProfile;
        Deleted = false;
    }
}
