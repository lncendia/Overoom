using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.Exceptions;
using Films.Application.Abstractions.Queries.Films;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Films;

/// <summary>
/// 
/// </summary>
/// <param name="context"></param>
public class GetFilmByIdQueryHandler(MongoDbContext context) : IRequestHandler<GetFilmByIdQuery, FilmDto>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FilmNotFoundException"></exception>
    public async Task<FilmDto> Handle(GetFilmByIdQuery request, CancellationToken cancellationToken)
    {
        var userWatchlist = request.UserId.HasValue
            ? await context.Users.AsQueryable()
                .Where(u => u.Id == request.UserId)
                .SelectMany(u => u.Watchlist.Select(w => w.FilmId))
                .ToListAsync(cancellationToken: cancellationToken)
            : [];

        // Начинаем агрегацию по коллекции фильмов
        var film = await context.Films.AsQueryable()
            .GroupJoin(
                context.Ratings.AsQueryable(),
                film => film.Id,
                rating => rating.FilmId,
                (film, ratings) => new
                {
                    Film = film,
                    // ReSharper disable once PossibleMultipleEnumeration
                    UserRating = ratings.Average(r => r.Score),
                    // ReSharper disable once PossibleMultipleEnumeration
                    UserRatingCount = ratings.Count(),
                    // ReSharper disable once PossibleMultipleEnumeration
                    UserScore = ratings.FirstOrDefault(r => r.UserId == request.UserId)
                }
            )
            .Select(x => new FilmDto
            {
                Id = x.Film.Id,
                Title = x.Film.Title,
                PosterKey = x.Film.PosterKey,
                Year = x.Film.Date.Year,
                UserRating = x.UserRating,
                RatingKp = x.Film.RatingKp,
                RatingImdb = x.Film.RatingImdb,
                Description = x.Film.Description,
                IsSerial = x.Film.Seasons != null && x.Film.Content == null,
                Genres = x.Film.Genres,
                UserRatingsCount = x.UserRatingCount,
                CanCreateRoom = x.Film.Content != null || (x.Film.Seasons != null && x.Film.Seasons.Any()),
                Content = x.Film.Content == null
                    ? null
                    : new MediaContentDto
                    {
                        Versions = x.Film.Content.Versions
                    },
                Seasons = x.Film.Seasons == null
                    ? null
                    : x.Film.Seasons.Select(s => new SeasonDto
                    {
                        Number = s.Number,
                        Episodes = s.Episodes.Select(e => new EpisodeDto
                        {
                            Number = e.Number,
                            Versions = e.Versions
                        }).ToList()
                    }).ToList(),
                Countries = x.Film.Countries,
                Directors = x.Film.Directors,
                ScreenWriters = x.Film.Screenwriters,
                Actors = x.Film.Actors,
                UserScore = x.UserScore == null ? null : x.UserScore.Score,
                InWatchlist = userWatchlist.Contains(x.Film.Id)
            })
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        // Возвращаем фильм
        return film ?? throw new FilmNotFoundException(request.Id);
    }
}