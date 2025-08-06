using AnyMapper;
using LanguageExt;
using Serilog;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Infrastructure.Repository.GoinWallet;

public class RepositoryGoinWalletRead : IRepositoryGoinWalletRead
{
    public RepositoryGoinWalletRead(ZagejmiContext context)
    {
        _context = context;
    }

    public async Task<Either<Failure, Domain.Community.Goin.GoinWallet?>> GetByIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        ModelGoinWallet? result;
        try
        {
            result = await _context
                .Set<ModelGoinWallet>()
                .FindAsync([id], cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        return Mapper.Map<Domain.Community.Goin.GoinWallet>(result);
    }

    private readonly ZagejmiContext _context;
}