using System.Drawing;

namespace Zagejmi.Write.Domain.Profile.Associate;

public sealed class AssociateProfile(
    Guid id,
    Image cardProfilePicture) : Entity<Guid>(id)
{
    #region AssociateProfile properties

    public Image CardProfilePicture = cardProfilePicture;

    #endregion
}