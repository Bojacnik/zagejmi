using System.Drawing;
using Zagejmi.Server.Domain.Entity;

namespace Zagejmi.Server.Write.Domain.Community.People.Associate;

public sealed class AssociateProfile(
    Guid id,
    Image cardProfilePicture
) : Entity<Guid>(id)
{
    #region AssociateProfile properties

    public Image CardProfilePicture = cardProfilePicture;

    #endregion
}