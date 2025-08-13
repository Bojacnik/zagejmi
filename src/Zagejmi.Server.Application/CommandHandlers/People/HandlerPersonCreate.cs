using LanguageExt;
using Zagejmi.Server.Application.Commands.Person;
using Zagejmi.Server.Domain.Auth;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.SharedKernel.Abstractions;
using Zagejmi.SharedKernel.Failures;
using Gender = Zagejmi.Server.Application.Commands.Person.Gender;
using PersonType = Zagejmi.Server.Write.Domain.Community.People.PersonType;

namespace Zagejmi.Server.Application.CommandHandlers.People;

public class HandlerPersonCreate : IRequestHandler<CommandPersonCreate, Either<Failure, Guid>>
{
    public HandlerPersonCreate(IRepositoryPersonWrite personRepository, IUnitOfWork unitOfWork,
        IHashHandler hashHandler, IMapper mapper)
    {
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _hashHandler = hashHandler;
        _map = mapper;
    }

    public async Task<Either<Failure, Guid>> Handle(CommandPersonCreate request, CancellationToken cancellationToken)
    {
        (string passwordHash, string salt) = _hashHandler.Hash(request.Password);

        Person person = new(Guid.NewGuid(),
            request.UserId,
            PersonType.Customer,
            new PersonalInformation(
                request.MailAddress.Address,
                request.UserName,
                request.FirstName,
                request.LastName,
                request.BirthDate,
                _map.Map<Gender, Write.Domain.Community.People.Gender>(request.Gender)),
            new PersonalStatistics(),
            [],
            null
        );

        _personRepository.Add(person);

        Either<Failure, Unit> result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Map(_ => person.Id);
    }

    private readonly IRepositoryPersonWrite _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashHandler _hashHandler;
    private readonly IMapper _map;
}