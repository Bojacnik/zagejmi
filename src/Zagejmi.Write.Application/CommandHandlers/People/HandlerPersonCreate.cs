using LanguageExt;

using Zagejmi.Write.Application.Commands.Person;
using Zagejmi.Write.Domain.Profile;

using DomainGender = Zagejmi.Write.Domain.Auth.Gender;

namespace Zagejmi.Write.Application.CommandHandlers.People;

public class HandlerPersonCreate : IHandlerRequest<CommandPersonCreate, Either<Failure, Guid>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryPersonWrite _repositoryPersonWrite;

    public HandlerPersonCreate(IRepositoryPersonWrite repositoryPersonWrite, IMapper mapper)
    {
        this._repositoryPersonWrite = repositoryPersonWrite;
        this._mapper = mapper;
    }

    public async Task<Either<Failure, Guid>> Handle(CommandPersonCreate request, CancellationToken cancellationToken)
    {
        bool personExists = await this._repositoryPersonWrite.ExistsByUserIdAsync(request.UserId);
        if (personExists)
        {
            return new FailureArgumentInvalidValue("A person profile for this user already exists.");
        }

        DomainGender domainGender = this._mapper.Map<Gender, DomainGender>(request.Gender);

        Either<Failure, Profile> personResult = Profile.Create(
            Guid.NewGuid(),
            request.UserId,
            request.MailAddress.Address,
            request.UserName,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            domainGender);

        if (personResult.IsLeft)
        {
            return personResult.LeftAsEnumerable().First();
        }

        Profile? person = personResult.RightAsEnumerable().First();
        return await this._repositoryPersonWrite.CreatePersonWithProjectionAsync(person);
    }
}