using System;

namespace Zagejmi.Contracts.Commands;

public sealed class UserCreateProfileCommand
{
    public Guid UserId { get; set; }

    public string ProfileType { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? ProfileImageDataUrl { get; set; }
}