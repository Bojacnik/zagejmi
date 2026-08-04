using System;

namespace Zagejmi.Application.Abstractions;

public interface IEmailVerificationLinkFactory
{
    string Create(Guid userId, string token);
}