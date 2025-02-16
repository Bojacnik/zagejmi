using System.Net.Mail;
using System.Drawing;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Domain.Community.User;

public enum PersonType
{
    Anon,
    Customer,
    VerifiedCustomer,
    Associate,
    VerifiedAssociate,
    Admin
}

public enum Gender
{
    Unknown,
    Male,
    Female,
    Other,
}

public enum VerificationType
{
    Id,
    Face,
    Personal,
}

public abstract class Verification
{
    public VerificationType Type { get; init; }
}

// images of identity identification front and back
public class IdVerification(Image imageFront, Image imageBack) : Verification
{
    public Image ImageFront = imageFront;
    public Image ImageBack = imageBack;
}

public class FaceVerification(string path) : Verification
{
    public string Path = path;
}

public class PersonalVerification(Person verifier, string note) : Verification
{
    public Person Verifier = verifier;
    public string? Note = note;
}

public record PersonalInfo(
    string FirstName,
    string LastName,
    MailAddress MailAddress,
    DateTime Birthday,
    Gender Gender,

    // Verification
    bool IsVerified,
    Verification? Verification
);

public record Statistics
{
    public ulong TotalScore { get; set; }
    public ulong Level { get; set; }

    // Time in seconds
    public ulong TimeTotal { get; set; }
    public ulong TimeWatching { get; set; }

    // Chats
    public uint ChatsSent { get; set; }

    // Money
    public ulong CzkSpent { get; set; }
    public ulong GoinSpent { get; set; }
    public ulong TransactionsAmount { get; set; }
    public ulong GiftsSent { get; set; }
}

public record Person(
    ulong Id,
    PersonalInfo PersonalInfo,
    Statistics Statistics,
    GoinWallet Wallet,
    PersonType Type
);