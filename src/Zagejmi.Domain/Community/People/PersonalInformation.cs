using Zagejmi.Domain.Entity;

namespace Zagejmi.Domain.Community.People;

public sealed record PersonalInformation : ValueObject
{
    public string? MailAddress;
    public readonly string? UserName;
    public string? FirstName;
    public string? LastName;
    public DateTime BirthDay;
    public Gender Gender;

    // Verification
    public bool IsVerified;
    public Verification.Verification? Verification;

    public PersonalInformation(
        string mailAddress,
        string userName,
        string firstName,
        string lastName,
        DateTime birthDay,
        Gender gender)
    {
        MailAddress = mailAddress;
        UserName = userName;
        FirstName = firstName;
        LastName = lastName;
        BirthDay = birthDay;
        Gender = gender;
        IsVerified = false;
        Verification = null;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return MailAddress;
        yield return UserName;
        yield return FirstName;
        yield return LastName;
        yield return BirthDay;
        yield return Gender;
        yield return IsVerified;
        yield return Verification;
    }
}