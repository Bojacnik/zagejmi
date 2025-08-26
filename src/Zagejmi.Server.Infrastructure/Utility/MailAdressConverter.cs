using System.Net.Mail;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Zagejmi.Server.Infrastructure.Utility;

// MailAddress conversion
public class MailAddressConverter() : ValueConverter<MailAddress, string>(address => address.ToString(),
    str => new MailAddress(str));