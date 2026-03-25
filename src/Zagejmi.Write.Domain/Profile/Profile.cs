using System;
using System.Collections.Generic;

using LanguageExt;

using Zagejmi.Contracts.Failures;
using Zagejmi.Write.Domain.Auth;
using Zagejmi.Write.Domain.Goin;
using Zagejmi.Write.Domain.Profile.Associate;

namespace Zagejmi.Write.Domain.Profile;

public sealed class Profile : Aggregate<Profile, Guid>
{
    // Private constructor for rehydration and creation
    private Profile(Guid id) : base(id)
    {
    }

    public static Either<Failure, Profile> Create(
        Guid personId,
        Guid userId,
        string email,
        string userName,
        string firstName,
        string lastName,
        DateTime birthDate,
        Gender gender)
    {
        if (userId == Guid.Empty)
        {
            return new FailureArgumentInvalidValue("A person must be associated with a user.");
        }

        Profile profile = new(personId);
        PersonalInformation personalInfo = new PersonalInformation(
            email,
            userName,
            firstName,
            lastName,
            birthDate,
            gender);

        profile.RaiseEvent(
            new EventPersonCreated(
                personId,
                userId,
                PersonType.Customer, // Assuming Customer is the default
                personalInfo,
                new PersonalStatistics(), // Assuming stats start empty
                new List<GoinWallet>(), // Assuming wallets start empty
                null // Assuming no associate profile at creation
            ));

        return profile;
    }

    protected override void Apply(IDomainEvent<Profile, Guid> evt)
    {
        switch (evt)
        {
            case EventPersonCreated e:
                this.OnPersonCreated(e);
                break;
        }
    }

    private void OnPersonCreated(EventPersonCreated e)
    {
        this.Id = e.PersonId;
        this.UserId = e.UserId;
        this.PersonType = e.PersonType;
        this.PersonalInformation = e.PersonalInformation;
        this.PersonalStatistics = e.PersonalStatistics;
        this.Wallets = e.Wallets;
        this.AssociateProfile = e.AssociateProfile;
        this.Deleted = false;
    }

    #region Person Properties

    public Guid UserId { get; private set; }

    public PersonType PersonType { get; private set; }

    public PersonalInformation PersonalInformation { get; private set; }

    public PersonalStatistics PersonalStatistics { get; private set; }

    public List<GoinWallet> Wallets { get; private set; } = new();

    public AssociateProfile? AssociateProfile { get; private set; }

    public bool Deleted { get; private set; }

    #endregion
}