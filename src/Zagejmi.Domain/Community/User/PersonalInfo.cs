using System.Net.Mail;
using SharedKernel;

namespace Zagejmi.Domain.Community.User;

public class PersonalInfo(
    MailAddress? mailAddress,
    string? userName,
    string? firstName,
    string? lastName,
    DateTime birthDay,
    Gender gender,

    // Verification
    bool isVerified,
    Verification.Verification? verification
) : ValueObject
{
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return mailAddress;
        yield return userName;
        yield return firstName;
        yield return lastName;
        yield return birthDay;
        yield return gender;
        yield return isVerified;
        yield return verification;
    }
}