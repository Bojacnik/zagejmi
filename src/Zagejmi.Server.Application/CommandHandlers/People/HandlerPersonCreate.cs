using LanguageExt;
using Zagejmi.Server.Application.Commands.Person;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.SharedKernel.Util;
using DomainGender = Zagejmi.Server.Write.Domain.Community.People.Gender;

namespace Zagejmi.Server.Application.CommandHandlers.People;

public class HandlerPersonCreate : IHandlerRequest<CommandPersonCreate, Either<Failure, Guid>>
{
    private readonly IRepositoryPersonWrite _repositoryPersonWrite;
    private readonly IMapper _mapper;

    public HandlerPersonCreate(IRepositoryPersonWrite repositoryPersonWrite, IMapper mapper)
    {
        _repositoryPersonWrite = repositoryPersonWrite;
        _mapper = mapper;
    }

    public async Task<Either<Failure, Guid>> Handle(CommandPersonCreate request, CancellationToken cancellationToken)
    {
        bool personExists = await _repositoryPersonWrite.ExistsByUserIdAsync(request.UserId);
        if (personExists)
        {
            return new FailureArgumentInvalidValue("A person profile for this user already exists.");
        }

        DomainGender domainGender = _mapper.Map<Gender, DomainGender>(request.Gender);

        Either<Failure, Person> personResult = Person.Create(
            Guid.NewGuid(),
            request.UserId,
            request.MailAddress.Address,
            request.UserName,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            domainGender
        );

        if (personResult.IsLeft)
        {
            return personResult.LeftAsEnumerable().First();
        }

        Person? person = personResult.RightAsEnumerable().First();
        return await _repositoryPersonWrite.CreatePersonWithProjectionAsync(person);
    }
}
