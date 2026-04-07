using System;

using SixLabors.ImageSharp;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Profile.Associate;

/// <summary>
///     Represents the profile of an associate, containing information such as the profile picture associated with the
///     card.
/// </summary>
public sealed class AssociateProfile : Entity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AssociateProfile" /> class.
    /// </summary>
    /// <param name="id">Unique identifier for the associate profile.</param>
    /// <param name="cardProfilePicture">
    ///     The profile picture associated with the card, represented as an <see cref="Image" />
    ///     object.
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
}