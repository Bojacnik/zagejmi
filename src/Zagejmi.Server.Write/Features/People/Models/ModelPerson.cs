using System.Text.Json.Serialization;

namespace Zagejmi.Server.Write.Features.People.Models;

[JsonSerializable(typeof(ModelPerson))]
public record ModelPerson
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

}