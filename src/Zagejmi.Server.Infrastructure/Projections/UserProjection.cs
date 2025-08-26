using System.ComponentModel.DataAnnotations;

namespace Zagejmi.Server.Infrastructure
{
    public class UserProjection
    {
        [Key]
        public Guid Id { get; init; }
        public required string UserName { get; init; }
        public required string Email { get; init; }
    }
}
