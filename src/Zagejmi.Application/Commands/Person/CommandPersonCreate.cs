using System.Net.Mail;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Application.Commands.Person;

public abstract record CommandPersonCreate(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender
);

public record CommandPersonCreateNew(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender) : CommandPersonCreate(MailAddress, UserName, FirstName, LastName, BirthDate, Gender);

public record CommandPersonCReateNewWithWallet(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender,
    GoinWallet GoinWallet) : CommandPersonCreate(MailAddress, UserName, FirstName, LastName, BirthDate, Gender);