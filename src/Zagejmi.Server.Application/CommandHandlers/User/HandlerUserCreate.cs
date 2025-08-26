using LanguageExt;
using Zagejmi.Server.Application.Commands.User;
using Zagejmi.Server.Domain.Auth;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.SharedKernel.Failures;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Zagejmi.Server.Application.CommandHandlers.User;

public class HandlerUserCreate : IHandlerRequest<CommandUserCreate, Either<Failure, Guid>>
{
    private readonly IRepositoryUserWrite _repositoryUserWrite;
    private readonly IHashHandler _hashHandler;

    public HandlerUserCreate(IRepositoryUserWrite repositoryUserWrite, IHashHandler hashHandler)
    {
        _repositoryUserWrite = repositoryUserWrite;
        _hashHandler = hashHandler;
    }

    public async Task<Either<Failure, Guid>> Handle(CommandUserCreate request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MailAddress) || !MailAddress.TryCreate(request.MailAddress, out _))
        {
            return new FailureArgumentInvalidValue($"The provided email address \'{request.MailAddress}\' is not valid.");
        }

        bool userExists = await _repositoryUserWrite.ExistsByUsernameOrEmailAsync(request.UserName, request.MailAddress);
        if (userExists)
        {
            return new FailureArgumentInvalidValue("A user with this username or email already exists.");
        }

        Guid userId;
        do
        {
            userId = Guid.NewGuid();
        } while (await _repositoryUserWrite.ExistsByIdAsync(userId));

        (string hash, string salt) = _hashHandler.Hash(request.Password);

        return await Domain.Auth.User.Create(
            userId,
            request.UserName,
            hash,
            salt,
            request.MailAddress)
        .BindAsync(user => _repositoryUserWrite.CreateUserAndPersonProjectionAsync(user));
    }
}
