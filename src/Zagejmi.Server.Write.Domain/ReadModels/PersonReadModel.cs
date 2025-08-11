namespace Zagejmi.Server.Write.Domain.ReadModels;

/// <summary>
/// A denormalized projection of a Person, optimized for fast queries.
/// This model would correspond to a 'People' table in the PostgreSQL read database.
/// </summary>
public class PersonReadModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}