﻿using SharedKernel;
using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User.Associate;
using Zagejmi.Domain.Events.People;

namespace Zagejmi.Domain.Community.User;

public sealed record Person : AggregateRoot<Person, Guid>
{
    #region Person Properties

    public PersonType PersonType { get; set; }
    public PersonalInformation PersonalInformation { get; init; }
    public PersonalStatistics PersonalStatistics { get; init; }
    public List<GoinWallet> Wallet { get; init; }

    #endregion

    #region Optional Properties

    public AssociateProfile? AssociateProfile { get; init; }

    #endregion

    public Person(
        Guid id,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        List<GoinWallet> wallet,
        AssociateProfile? associateProfile) : base(id)
    {
        PersonType = personType;
        PersonalInformation = personalInformation;
        PersonalStatistics = personalStatistics;
        Wallet = wallet;
        AssociateProfile = associateProfile;
    }

    protected override void Apply(IDomainEvent<Person, Guid> evt)
    {
        switch (evt.EventType)
        {
            case EventTypeDomain.PersonCreated:
                var eventPersonCreated = (EventPersonCreated)evt;
                if (eventPersonCreated.AggregateId == Id)
                {
                    throw new Exception("Person with this ID already exists");
                }

                eventPersonCreated.Apply(this);
                break;
            case EventTypeDomain.PersonUpdated:
                var eventPersonUpdated = (EventPersonUpdated)evt;
                if (eventPersonUpdated.AggregateId == Id)
                {
                    eventPersonUpdated.Apply(this);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}