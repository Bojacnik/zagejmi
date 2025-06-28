using System.Drawing;
using SharedKernel;

namespace Zagejmi.Domain.Community.User.Associate;

public class AssociateProfile : ValueObject
{
    public Person Owner;
    public Image CardProfilePicture;
    
    public AssociateProfile(Person owner, Image cardProfilePicture)
    {
        Owner = owner;
        CardProfilePicture = cardProfilePicture;
    }

    // TODO: Add a lot of stuff :) (i.e personality, astro-sign, badges, animals, photos, ...)
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Owner;
        yield return CardProfilePicture;
    }
}