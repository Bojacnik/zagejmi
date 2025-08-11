using System.Net.Mail;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Zagejmi.Server.Write.Infrastructure.Utility;

// MailAddress conversion
public class MailAddressConverter() : ValueConverter<MailAddress, string>(address => address.ToString(),
    str => new MailAddress(str));