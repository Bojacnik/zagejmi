using Zagejmi.Server.Application.Commands.Person;

namespace Zagejmi.Server.Infrastructure.Projections;

public class PersonProjection
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Email { get; set; } = "";
    public string? UserName { get; set; } = "";
    public string? FirstName { get; set; } = "";
    public string? LastName { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public int GoinAmount { get; set; }
}