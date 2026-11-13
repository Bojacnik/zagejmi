using System;
using System.Collections.Generic;

using Zagejmi.Domain.Profiles.Abstractions;

namespace Zagejmi.Domain.Profiles.Players;

/// <summary>
///     Represents the profile of a player (Hráč/Hráčka): a user who offers services to customers. Any user can create
///     one in addition to their customer profile and switch between them.
/// </summary>
public sealed class PlayerProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlayerProfile" /> class.
    /// </summary>
    /// <param name="id">The profile identifier.</param>
    /// <param name="userId">The identifier of the owning user.</param>
    /// <param name="displayName">The display name for the player profile.</param>
    public PlayerProfile(Guid id, Guid userId, string displayName)
        : base(id, userId, displayName)
    {
    }

    /// <summary>
    ///     Gets or sets the profile picture associated with the card, referenced by the ID of an image blob. This property holds the visual representation of the player's profile picture,
    ///     which can be used for display purposes in the application. The image can be in various formats (e.g., JPEG, PNG)
    ///     and is typically stored as a binary object in the database. The <see cref="CardProfilePicture" /> property allows
    ///     the application to retrieve and display the player's profile picture when needed, such as in user interfaces or
    ///     profile views.
    /// </summary>
    public Guid? CardProfilePicture { get; set; }

    /// <summary>
    ///     Gets or sets the banner picture associated with the player's profile, referenced by the ID of an image blob. This property holds the visual representation of the player's banner picture, which can be used for
    ///     display purposes in the application. The banner picture is typically a larger image that serves as a background or
    ///     header for the player's profile, providing additional visual context and branding. The
    ///     <see cref="BannerPicture" /> property allows the application to retrieve and display the player's banner picture
    ///     when needed, such as in user interfaces or profile views. The image is in
    ///     webp and is typically stored in minio.
    /// </summary>
    public Guid? BannerPicture { get; set; }

    /// <summary>
    ///     Gets or sets the background picture associated with the player's services, referenced by the id of
    ///     its stored image. This property holds the visual representation of the player's background picture for their services,
    ///     which can be used for display purposes in the application. The background picture is typically a larger image that
    ///     serves as a backdrop for the player's services, providing additional visual context and branding. The
    ///     <see cref="BackgroundServicesPicture" /> property allows the application to retrieve and display the player's
    ///     background picture for their services when needed, such as in user interfaces or service views. The image is in
    ///     webp and is typically stored in minio.
    /// </summary>
    public Guid? BackgroundServicesPicture { get; set; }

    /// <summary>
    ///     Gets or sets the description of the player's profile. This property provides a textual representation of the
    ///     player's profile, allowing them to share information about themselves, their interests, or any other relevant
    ///     details. The description can be used to give context to the player's profile, helping other users understand
    ///     more about them. It can be displayed in user interfaces, profile views, or any other relevant sections of the
    ///     application where the player's profile is presented. The description can be a brief summary or a more detailed
    ///     narrative, depending on the player's preference and the application's requirements.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets the array of services associated with the player's profile. This property holds a collection of
    ///     services that the player offers or is involved in, allowing them to showcase their expertise and offerings to
    ///     other users. Each service in the array can contain relevant information such as the service name, description,
    ///     pricing, and any other pertinent details that help users understand what the player provides. The
    ///     <see cref="Services" /> property enables the application to display and manage the player's services
    ///     effectively, facilitating interactions and transactions between the player and other users within the platform.
    /// </summary>
    public List<Guid> Services { get; private set; } = [];
}
