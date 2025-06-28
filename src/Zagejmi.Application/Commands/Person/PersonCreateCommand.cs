using System.Net.Mail;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Application.Commands.Person;

public abstract record PersonCreateCommand(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender
);

public record PersonCreateNewCommand(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender) : PersonCreateCommand(MailAddress, UserName, FirstName, LastName, BirthDate, Gender);

public record PersonCReateNewWithWalletCommand(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender,
    GoinWallet GoinWallet) : PersonCreateCommand(MailAddress, UserName, FirstName, LastName, BirthDate, Gender);