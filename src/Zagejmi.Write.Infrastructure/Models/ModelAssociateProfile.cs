using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Zagejmi.Write.Infrastructure.Models;

[Table("AssociateProfile")]
public class ModelAssociateProfile
{
    [Key] public Guid Id { get; set; }
    public Image CardProfilePicture { get; set; }

    public ModelAssociateProfile() {}
}
