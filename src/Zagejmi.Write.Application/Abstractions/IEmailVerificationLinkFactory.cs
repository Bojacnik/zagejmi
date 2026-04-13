using System;

namespace Zagejmi.Write.Application.Abstractions;

public interface IEmailVerificationLinkFactory
{
    string Create(Guid userId, string token);
}