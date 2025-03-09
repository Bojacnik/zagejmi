using System.Drawing;

namespace Zagejmi.Domain.Community.User.Associate;

public record AssociateProfile(Image CardProfilePicture)
{
    public required Person Owner;

    public Image CardProfilePicture = CardProfilePicture;

    // TODO: Add a lot of stuff :) (i.e personality, astro-sign, badges, animals, photos, ...)
}