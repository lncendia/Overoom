using Films.Application.Abstractions.Common.Exceptions;
using Films.Application.Abstractions.Queries.Films;
using Films.Application.Abstractions.Queries.Films.DTOs;
using Films.Domain.Abstractions.Interfaces;
using Films.Domain.Films.Specifications;
using MediatR;

namespace Films.Application.Services.QueryHandlers.Films;

public class UserWatchlistQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UserWatchlistQuery, IReadOnlyCollection<FilmShortDto>>
{
    public async Task<IReadOnlyCollection<FilmShortDto>> Handle(UserWatchlistQuery request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.Id);

        if (user == null) throw new UserNotFoundException();

        var specification = new FilmsByIdsSpecification(user.Watchlist.Select(x => x.FilmId));
        var films = await unitOfWork.FilmRepository.Value.FindAsync(specification);

        var watchlist = user.Watchlist.OrderByDescending(x => x.Date).Select(x => x.FilmId).ToList();
        
        // Преобразуем фильмы в список DTO фильмов 
        return films.OrderBy(film => watchlist.IndexOf(film.Id)).Select(Mapper.Map).ToArray();
    }
}