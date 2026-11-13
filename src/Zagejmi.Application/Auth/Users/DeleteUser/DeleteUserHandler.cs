using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;

namespace Zagejmi.Application.Auth.Users.DeleteUser;

public class DeleteUserHandler : IHandlerRequest<DeleteUserCommand, Guid>
{
    private readonly IUserRepository userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        await this.userRepository.RemoveAsync(request.Id, cancellationToken);
        return request.Id;
    }
}