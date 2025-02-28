namespace Zagejmi.Domain.Community.User.Verification;

public class PersonalVerification(Person verifier, string note) : Verification
{
    public Person Verifier = verifier;
    public string? Note = note;
}