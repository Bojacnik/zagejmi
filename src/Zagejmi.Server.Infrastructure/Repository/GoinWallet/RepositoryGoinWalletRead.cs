using LanguageExt;
using Serilog;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.SharedKernel.Util;

namespace Zagejmi.Server.Infrastructure.Repository.GoinWallet;

public class RepositoryGoinWalletRead : IRepositoryGoinWalletRead
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryGoinWalletRead(ZagejmiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

        if (result is null) 
        {
            return (Domain.Community.Goin.GoinWallet?)null;
        }

        var mappedWallet = _mapper.Map<ModelGoinWallet, Domain.Community.Goin.GoinWallet>(result);
        if (mappedWallet is null)
        {
            return new FailureMapper("Mapping from ModelGoinWallet to GoinWallet resulted in null.");
        }

        return mappedWallet;
    }
}
