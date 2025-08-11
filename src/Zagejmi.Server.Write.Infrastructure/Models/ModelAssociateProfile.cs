using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Zagejmi.Server.Write.Infrastructure.Models;

[Table("AssociateProfile")]
public class ModelAssociateProfile
{
    [Key] public uint Id { get; init; }
    public Image CardProfilePicture { get; init; }

    public ModelAssociateProfile(
        uint id,
        Image cardProfilePicture)
    {
        Id = id;
        CardProfilePicture = cardProfilePicture;
    }
}