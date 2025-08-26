namespace Zagejmi.Server.Application.Commands.User;

public class CommandUserCreate
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string MailAddress { get; set; } = "";
}
