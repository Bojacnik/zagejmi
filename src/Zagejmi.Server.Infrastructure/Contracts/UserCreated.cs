using System;

namespace Zagejmi.Server.Infrastructure.Contracts
{
    public record UserCreated(Guid Id, string UserName, string Email);
}
