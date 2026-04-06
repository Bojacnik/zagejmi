using System;
using System.Collections.Generic;

using LanguageExt;

using Zagejmi.Shared.Failures;
using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Domain.Auth;
using Zagejmi.Write.Domain.Goin;
using Zagejmi.Write.Domain.Profile.Associate;

namespace Zagejmi.Write.Domain.Profile;

public sealed class Profile : Aggregate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Profile" /> class with the specified unique identifier.
    ///     This constructor is private to enforce the use of the static factory method for creating instances of the
    ///     Profile aggregate. The factory method ensures that all necessary validations and event raisings are performed
    ///     during the creation process, maintaining the integrity of the aggregate's state and adhering to the principles
    ///     of domain-driven design.
    /// </summary>
    /// <param name="id">Unique identifier for the profile aggregate.</param>
    private Profile(Guid id)
        : base(id)
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
        Profile profile = new(personId);
        PersonalInformation personalInfo;
        personalInfo = new PersonalInformation(
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