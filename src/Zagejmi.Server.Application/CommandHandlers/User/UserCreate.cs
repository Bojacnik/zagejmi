using LanguageExt;
using Zagejmi.Server.Application.Commands.User;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Application.CommandHandlers.User;

public class UserCreate(
    IRepositoryUserWrite repository,
    IUnitOfWork unitOfWork) : IHandlerRequest<CommandUserCreate, Either<Failure, Guid>>
{
    public async Task<Either<Failure, Guid>> Handle(CommandUserCreate request, CancellationToken cancellationToken)
    {
        // 1. Attempt to add the user to the repository.
        // repository.Add returns Either<Failure, Guid>.
        // If this operation results in a Failure (Left), the BindAsync will short-circuit
        // and the Failure will be returned immediately.
        return await repository
            .Add(request.UserName, request.HashedPassword, request.MailAddress)
            .BindAsync(async userId =>
            {
                // 2. If the user was successfully added (Right(userId)), proceed to save changes.
                // We assume unitOfWork.SaveChangesAsync() also returns Task<Either<Failure, Unit>>
                // to maintain the monadic flow. If it fails, this Failure will propagate.
                Either<Failure, Unit> saveResult = await unitOfWork.SaveChangesAsync(CancellationToken.None);

                // 3. If changes were saved successfully (Right(Unit)), map the Unit to the original userId.
                // If saveResult was a Failure (Left), this Map operation will not execute,
                // and the Failure will be propagated.
                return saveResult.Map(_ => userId);
            });
    }
}