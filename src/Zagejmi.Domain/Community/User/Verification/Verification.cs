using SharedKernel;
using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Community.User.Verification;

public abstract class Verification : ValueObject
{
    public VerificationType Type { get; init; }
    
}
