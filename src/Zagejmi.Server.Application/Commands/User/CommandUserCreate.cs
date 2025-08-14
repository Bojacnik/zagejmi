namespace Zagejmi.Server.Application.Commands.User;

public class CommandUserCreate
{
    public string UserName { get; set; } = "";
    public string HashedPassword { get; set; } = "";
    public string MailAddress { get; set; } = "";
}