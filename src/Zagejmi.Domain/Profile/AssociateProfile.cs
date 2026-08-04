using System;
using System.Collections.Generic;

using SixLabors.ImageSharp;

using Zagejmi.Domain.Abstractions;
using Zagejmi.Domain.Goin;

namespace Zagejmi.Domain.Profile;

/// <summary>
///     Represents the profile of an associate, containing information such as the profile picture associated with the
///     card.
/// </summary>
public sealed class AssociateProfile : Abstractions.Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AssociateProfile" /> class.
    /// </summary>
    /// <param name="id">Unique identifier for the associate profile.</param>
    /// <param name="cardProfilePicture">
    ///     The profile picture associated with the card, represented as an <see cref="Image" /> object.
    /// </param>
    public AssociateProfile(Guid id, Image cardProfilePicture)
        : base(id)
    {
        this.CardProfilePicture = cardProfilePicture;
    }

    /// <summary>
    ///     Gets the profile picture associated with the card, represented as an <see cref="Image" />
    ///     object. This property holds the visual representation of the associate's profile picture,
    ///     which can be used for display purposes in the application. The image can be in various formats (e.g., JPEG, PNG)
    ///     and is typically stored as a binary object in the database. The <see cref="CardProfilePicture" /> property allows
    ///     the application to retrieve and display the associate's profile picture when needed, such as in user interfaces or
    ///     profile views.
    /// </summary>
    public Image CardProfilePicture { get; private set; }

    /// <summary>
    ///     Gets or sets the banner picture associated with the associate's profile, represented as an <see cref="Image" />
    ///     object. This property holds the visual representation of the associate's banner picture, which can be used for
    ///     display purposes in the application. The banner picture is typically a larger image that serves as a background or
    ///     header for the associate's profile, providing additional visual context and branding. The
    ///     <see cref="BannerPicture" /> property allows the application to retrieve and display the associate's banner picture
    ///     when needed, such as in user interfaces or profile views. The image can be in various formats (e.g., JPEG, PNG) and
    ///     is typically stored as a binary object in the database.
    /// </summary>
    public Image BannerPicture { get; set; }

    /// <summary>
    ///     Gets or sets the background picture associated with the associate's services, represented as an
    ///     <see cref="Image" />
    ///     object. This property holds the visual representation of the associate's background picture for their services,
    ///     which can be used for display purposes in the application. The background picture is typically a larger image that
    ///     serves as a backdrop for the associate's services, providing additional visual context and branding. The
    ///     <see cref="BackgroundServicesPicture" /> property allows the application to retrieve and display the associate's
    ///     background picture for their services when needed, such as in user interfaces or service views. The image can be in
    ///     various formats (e.g., JPEG, PNG) and is typically stored as a binary object in the database.
    /// </summary>
    public Image BackgroundServicesPicture { get; set; }

    /// <summary>
    ///     Gets or sets the description of the associate's profile. This property provides a textual representation of the
    ///     associate's profile, allowing them to share information about themselves, their interests, or any other relevant
    ///     details. The description can be used to give context to the associate's profile, helping other users understand
    ///     more about them. It can be displayed in user interfaces, profile views, or any other relevant sections of the
    ///     application where the associate's profile is presented. The description can be a brief summary or a more detailed
    ///     narrative, depending on the associate's preference and the application's requirements.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the array of services associated with the associate's profile. This property holds a collection of
    ///     services that the associate offers or is involved in, allowing them to showcase their expertise and offerings to
    ///     other users. Each service in the array can contain relevant information such as the service name, description,
    ///     pricing, and any other pertinent details that help users understand what the associate provides. The
    ///     <see cref="AssociateServices" /> property enables the application to display and manage the associate's services
    ///     effectively, facilitating interactions and transactions between the associate and other users within the platform.
    /// </summary>
    public AssociateService[] AssociateServices { get; private set; } = new();

    protected override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            // TODO: Implement domain event handlers
            default:
                throw new ArgumentOutOfRangeException(nameof(domainEvent), domainEvent, "Unknown domain event type");
        }
    }

    public override Abstractions.Profile Create(Guid userId, string displayName, GoinWallet walletId)
    {
        throw new NotImplementedException();
    }

    public override Abstractions.Profile? FromHistory(Guid id, IReadOnlyList<IDomainEvent> history)
    {
        throw new NotImplementedException();
    }
}