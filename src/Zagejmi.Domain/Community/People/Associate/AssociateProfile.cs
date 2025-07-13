using System.Drawing;
using SharedKernel;

namespace Zagejmi.Domain.Community.People.Associate;

public sealed record AssociateProfile : Entity<Guid>
{
    #region AssociateProfile properties

    public Image CardProfilePicture;

    #endregion

    public AssociateProfile(
        Guid id, 
        Image cardProfilePicture) : base(id)
    {
        CardProfilePicture = cardProfilePicture;
    }
}