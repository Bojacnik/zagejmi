using System;

namespace Zagejmi.Contracts.Commands;

public sealed class UserUpdateProfileImageCommand
{
    public Guid UserId { get; set; }

    public Guid ProfileId { get; set; }

    public string ProfileImageDataUrl { get; set; } = string.Empty;
}