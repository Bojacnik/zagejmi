using System.Net.Mail;

namespace Zagejmi.Domain.Community.User;

public record PersonalInfo(
    MailAddress MailAddress,
    string FirstName,
    string LastName,
    DateTime BirthDay,
    Gender Gender,

    // Verification
    bool IsVerified,
    Verification.Verification? Verification
);