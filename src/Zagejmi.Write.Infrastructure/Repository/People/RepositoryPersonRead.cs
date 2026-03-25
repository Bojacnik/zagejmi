using LanguageExt;

using Microsoft.EntityFrameworkCore;

using Serilog;

using Zagejmi.Write.Domain.Profile;
using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Models;

namespace Zagejmi.Write.Infrastructure.Repository.People;

public class RepositoryPersonRead : IRepositoryPersonRead
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryPersonRead(ZagejmiContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<Either<Failure, Profile>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        ModelPerson? person;
        try
        {
            person = await this._context.Set<ModelPerson>()
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

        var mappedPerson = this._mapper.Map<ModelPerson, Profile>(person);
        if (mappedPerson is null)
        {
            return new FailureMapper("Mapping from ModelPerson to Person resulted in null.");
        }

        return mappedPerson;
    }

    public async Task<Either<Failure, Profile>> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
    {
        ModelPerson? person;
        try
        {
            person = await this._context.Set<ModelPerson>()
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

        var mappedPerson = this._mapper.Map<ModelPerson, Profile>(person);
        if (mappedPerson is null)
        {
            return new FailureMapper("Mapping from ModelPerson to Person resulted in null.");
        }

        return mappedPerson;
    }
}