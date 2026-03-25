using System;
using System.Collections.Generic;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Auth;

/// <summary>
///     Represents the personal information of a profile, including contact details, name, birthdate, gender, and
///     verification status.
/// </summary>
public sealed record PersonalInformation : ValueObject
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PersonalInformation" /> class with the specified mail address, first
    ///     name, last name, birthday, and gender. This constructor allows for the creation of a complete personal information
    ///     record for a profile, ensuring that all relevant details are captured and can be used for various purposes such as
    ///     communication, personalization, and demographic analysis.
    /// </summary>
    /// <param name="mailAddress">
    ///     The email address associated with the profile, used for communication and identification
    ///     purposes.
    /// </param>
    /// <param name="firstName">The first name of the profile, which is used for personalization and display purposes.</param>
    /// <param name="lastName">The last name of the profile, which is used for personalization and display purposes.</param>
    /// <param name="birthDay">The birthdate of the profile, which can be used for personalization and demographic purposes.</param>
    /// <param name="gender">The gender of the profile, which can be used for personalization and demographic purposes.</param>
    public PersonalInformation(
        string mailAddress,
        string firstName,
        string lastName,
        DateOnly birthDay,
        Gender gender)
    {
        this.MailAddress = mailAddress;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.BirthDay = birthDay;
        this.Gender = gender;
    }

    /// <summary>
    ///     Gets the email address associated with the profile, used for communication and identification purposes.
    /// </summary>
    public string MailAddress { get; }

    /// <summary>
    ///     Gets the first name of the profile, which is used for personalization and display purposes.
    /// </summary>
    public string? FirstName { get; }

    /// <summary>
    ///     Gets the last name of the profile, which is used for personalization and display purposes.
    /// </summary>
    public string? LastName { get; }

    /// <summary>
    ///     Gets the username of the profile, which is used for identification and login purposes.
    /// </summary>
    public DateOnly BirthDay { get; }

    /// <summary>
    ///     Gets the gender of the profile, which can be used for personalization and demographic purposes.
    /// </summary>
    public Gender Gender { get; }

    /// <summary>
    ///     Returns an enumeration of the atomic values that define the equality of this value object. In this case, the mail
    ///     address, first name, last name, birth, and gender properties are used to determine equality, ensuring that two
    ///     instances of <see cref="PersonalInformation" /> are considered equal if all of these properties have the same
    ///     values. This allows for value-based equality comparisons, which is a key characteristic of value objects in
    ///     domain-driven design.
    /// </summary>
    /// <returns>
    ///     An enumeration of the atomic values that define the equality of this value object, which includes the mail
    ///     address, first name, last name, birth, and gender properties.
    /// </returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.MailAddress;
        yield return this.FirstName;
        yield return this.LastName;
        yield return this.BirthDay;
        yield return this.Gender;
    }
}