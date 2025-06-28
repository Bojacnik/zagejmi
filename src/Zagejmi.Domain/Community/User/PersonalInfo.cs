using System.Net.Mail;
using SharedKernel;

namespace Zagejmi.Domain.Community.User;

public class PersonalInfo : ValueObject
{
    public MailAddress? MailAddress;
    public string? UserName;
    public string? FirstName;

    public string? LastName;
    public DateTime BirthDay;
    public Gender Gender;

    // Verification
    public bool IsVerified;
    public Verification.Verification? Verification;

    public PersonalInfo(
        MailAddress mailAddress,
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