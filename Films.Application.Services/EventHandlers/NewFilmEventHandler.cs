using Films.Application.Abstractions.Common.Exceptions;
using Films.Application.Abstractions.Common.Interfaces;
using Films.Domain.Abstractions.Interfaces;
using Films.Domain.Films;
using MediatR;
using Films.Domain.Films.Events;
using Films.Domain.Films.Specifications;
using Films.Domain.Films.Specifications.Visitor;
using Films.Domain.Specifications;

namespace Films.Application.Services.EventHandlers;

public class NewFilmEventHandler(IUnitOfWork unitOfWork, IPosterService posterService)
    : INotificationHandler<NewFilmDomainEvent>
{
    public async Task Handle(NewFilmDomainEvent notification, CancellationToken cancellationToken)
    {
        var filmSpec = new AndSpecification<Film, IFilmSpecificationVisitor>(
            new FilmsByTitleSpecification(notification.Film.Title),
            new FilmsByYearsSpecification(notification.Film.Year, notification.Film.Year));

        var count = await unitOfWork.FilmRepository.Value.FindAsync(filmSpec);
        if (count.Count > 0)
        {
            await posterService.DeleteAsync(notification.Film.PosterUrl);
            throw new FilmAlreadyExistsException();
        }
    }
}