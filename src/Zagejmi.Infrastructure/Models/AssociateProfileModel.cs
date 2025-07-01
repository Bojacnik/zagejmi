using System.Drawing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Infrastructure.Models;

[Table("AssociateProfile")]
public class AssociateProfileModel
{
    [Key] public uint Id { get; init; }
    public Image CardProfilePicture { get; init; }

    public AssociateProfileModel(
        uint id,
        Image cardProfilePicture)
    {
        Id = id;
        CardProfilePicture = cardProfilePicture;
    }
}