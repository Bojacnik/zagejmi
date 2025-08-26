using MassTransit;
using Microsoft.EntityFrameworkCore;
using Zagejmi.Server.Domain.Events.Auth;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Write.Domain.Community.People;

namespace Zagejmi.Server.Infrastructure.Projections
{
    public class PersonProjectionHandler : IConsumer<EventPersonCreated>
    {
        private readonly ZagejmiContext _context;

        public PersonProjectionHandler(ZagejmiContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<EventPersonCreated> context)
        {
            var existingProjection = await _context.PersonProjections
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == context.Message.PersonId, context.CancellationToken);

            if (existingProjection != null)
            {
                return;
            }

            PersonProjection personProjection = new PersonProjection
            {
                Id = context.Message.PersonId,
                UserId = context.Message.UserId,
                Email = context.Message.PersonalInformation.MailAddress,
                UserName = context.Message.PersonalInformation.UserName,
                FirstName = context.Message.PersonalInformation.FirstName,
                LastName = context.Message.PersonalInformation.LastName,
                BirthDate = context.Message.PersonalInformation.BirthDay,
                Gender = context.Message.PersonalInformation.Gender switch
                {
                    Gender.Unknown => Application.Commands.Person.Gender.Unknown,
                    Gender.Male => Application.Commands.Person.Gender.Male,
                    Gender.Female => Application.Commands.Person.Gender.Female,
                    Gender.Other => Application.Commands.Person.Gender.Other,
                    _ => throw new ArgumentOutOfRangeException()
                },
                GoinAmount = 0
            };

            _context.PersonProjections.Add(personProjection);
            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
