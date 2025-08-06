using AnyMapper;
using LanguageExt;
using Serilog;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Infrastructure.Repository.People;

public class RepositoryPersonRead : IRepositoryPersonRead
{
    public RepositoryPersonRead(ZagejmiContext context)
    {
        _context = context;
    }

    public async Task<Either<Failure, Domain.Community.People.Person.Person>> GetByEmailAsync(
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

        return Mapper.Map<Domain.Community.People.Person.Person>(person);
    }

    public async Task<Either<Failure, Domain.Community.People.Person.Person>> GetByUsernameAsync(
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

        return Mapper.Map<Domain.Community.People.Person.Person>(person);
    }

    private readonly ZagejmiContext _context;
}