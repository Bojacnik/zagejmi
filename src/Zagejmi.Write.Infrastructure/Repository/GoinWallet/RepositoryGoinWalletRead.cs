using LanguageExt;

using Serilog;

using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Models;

namespace Zagejmi.Write.Infrastructure.Repository.GoinWallet;

public class RepositoryGoinWalletRead : IRepositoryGoinWalletRead
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryGoinWalletRead(ZagejmiContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<Either<Failure, Domain.Goin.GoinWallet?>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        ModelGoinWallet? result;
        try
        {
            result = await this._context
                .Set<ModelGoinWallet>()
                .FindAsync([id], cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        if (result is null)
        {
            return (Domain.Goin.GoinWallet?)null;
        }

        var mappedWallet = this._mapper.Map<ModelGoinWallet, Domain.Goin.GoinWallet>(result);
        if (mappedWallet is null)
        {
            return new FailureMapper("Mapping from ModelGoinWallet to GoinWallet resulted in null.");
        }

        return mappedWallet;
    }
}