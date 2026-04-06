using System.Net.Mail;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Zagejmi.Write.Infrastructure.Utility;

/// <summary>
///     A value converter for converting between MailAddress objects and their string representations for storage in a
///     database.
/// </summary>
public class MailAddressConverter() : ValueConverter<MailAddress, string>(
    address => address.ToString(),
    str => new MailAddress(str));