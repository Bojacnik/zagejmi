using System.Drawing;
using SharedKernel;

namespace Zagejmi.Domain.Community.User.Associate;

public class AssociateProfile : Entity<uint>
{
    #region AssociateProfile properties

    public Image CardProfilePicture;

    #endregion

    public AssociateProfile(
        uint id, 
        Image cardProfilePicture) : base(id)
    {
        CardProfilePicture = cardProfilePicture;
    }
}