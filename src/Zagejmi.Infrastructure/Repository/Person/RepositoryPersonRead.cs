using AnyMapper;
using LanguageExt;
using Serilog;
using SharedKernel.Failures;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Repository.Person;

public class RepositoryPersonRead : IRepositoryPersonRead
{
    public RepositoryPersonRead(ZagejmiContext context)
    {
        _context = context;
    }

    public async Task<Either<Failure, Domain.Community.User.Person>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        PersonModel? person;
        try
        {
            person = await _context.Set<PersonModel>().FindAsync([email], cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        if (person == null)
        {
            Log.Information("Person not found");
            return new FailureDatabaseEntityNotFound("Person not found");
        }

        return Mapper.Map<Domain.Community.User.Person>(person);
    }

    public async Task<Either<Failure, Domain.Community.User.Person>> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
    {
        PersonModel? person;
        try
        {
            person = await _context.Set<PersonModel>().FindAsync([username], cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        if (person == null)
        {
            Log.Information("Person not found");
            return new FailureDatabaseEntityNotFound("Person not found");
        }

        return Mapper.Map<Domain.Community.User.Person>(person);
    }

    private readonly ZagejmiContext _context;
}