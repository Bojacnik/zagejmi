using AnyMapper;
using LanguageExt;
using Serilog;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Repository;
using Zagejmi.Server.Write.Infrastructure.Ctx;
using Zagejmi.Server.Write.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Infrastructure.Repository.People;

public class RepositoryPersonRead : IRepositoryPersonRead
{
    public RepositoryPersonRead(ZagejmiContext context)
    {
        _context = context;
    }

    public async Task<Either<Failure, Person>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        ModelPerson? person;
        try
        {
            person = await _context.Set<ModelPerson>().FindAsync([email], cancellationToken);
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

        return Mapper.Map<Person>(person);
    }

    public async Task<Either<Failure, Person>> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
    {
        ModelPerson? person;
        try
        {
            person = await _context.Set<ModelPerson>().FindAsync([username], cancellationToken);
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

        return Mapper.Map<Person>(person);
    }

    private readonly ZagejmiContext _context;
}