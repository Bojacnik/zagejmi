using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.SharedKernel.Util;

namespace Zagejmi.Server.Infrastructure.Repository.People;

public class RepositoryPersonRead : IRepositoryPersonRead
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryPersonRead(ZagejmiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Either<Failure, Person>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        ModelPerson? person;
        try
        {
            person = await _context.Set<ModelPerson>()
                .Include(p => p.PersonalInformation)
                .FirstOrDefaultAsync(p => p.PersonalInformation.MailAddress == email, cancellationToken);
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

        var mappedPerson = _mapper.Map<ModelPerson, Person>(person);
        if (mappedPerson is null)
        {
            return new FailureMapper("Mapping from ModelPerson to Person resulted in null.");
        }

        return mappedPerson;
    }

    public async Task<Either<Failure, Person>> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
    {
        ModelPerson? person;
        try
        {
            person = await _context.Set<ModelPerson>()
                .Include(p => p.PersonalInformation)
                .FirstOrDefaultAsync(p => p.PersonalInformation.UserName == username, cancellationToken);
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

        var mappedPerson = _mapper.Map<ModelPerson, Person>(person);
        if (mappedPerson is null)
        {
            return new FailureMapper("Mapping from ModelPerson to Person resulted in null.");
        }

        return mappedPerson;
    }
}
