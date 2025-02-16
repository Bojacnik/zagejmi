using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Community.Goin;

public record GoinTransaction(
    ulong Id,
    Person Sender,
    Person Receiver,
    Goin Goin
);