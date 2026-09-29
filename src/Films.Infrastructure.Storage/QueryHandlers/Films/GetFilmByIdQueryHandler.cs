using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.Exceptions;
using Films.Application.Abstractions.Queries.Films;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Infrastructure.Storage.QueryHandlers.Films;

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
    List<Guid>? userWatchlist = request.UserId.HasValue
      ? await context.Users.AsQueryable()
        .Where(u => u.Id == request.UserId)
        .SelectMany(u => u.Watchlist.Select(w => w.FilmId))
        .ToListAsync(cancellationToken: cancellationToken)
      : [];

    double? userScore = request.UserId.HasValue
      ? await context.Ratings.AsQueryable()
        .Where(r => r.UserId == request.UserId && r.FilmId == request.Id)
        .Select(r => (double?)r.Score)
        .FirstOrDefaultAsync(cancellationToken)
      : null;

    FilmDto? film = await context.Films.AsQueryable()
      .Where(f => f.Id == request.Id)
      .Select(x => new FilmDto
      {
        Id = x.Id,
        Title = x.Title,
        PosterKey = x.PosterKey,
        Year = x.Date.Year,
        UserRating = x.UserRatingsCount == 0 ? null : x.UserRatingsSum / x.UserRatingsCount,
        RatingKp = x.RatingKp,
        RatingImdb = x.RatingImdb,
        Description = x.Description,
        IsSerial = x.Seasons != null && x.Content == null,
        Genres = x.Genres,
        UserRatingsCount = x.UserRatingsCount,
        CanCreateRoom = x.Content != null || (x.Seasons != null && x.Seasons.Any()),
        Content = x.Content == null
          ? null
          : new MediaContentDto
          {
            Versions = x.Content.Versions
          },
        Seasons = x.Seasons == null
          ? null
          : x.Seasons.Select(s => new SeasonDto
          {
            Number = s.Number,
            Episodes = s.Episodes.Select(e => new EpisodeDto
            {
              Number = e.Number,
              Versions = e.Versions
            }).ToList()
          }).ToList(),
        Countries = x.Countries,
        Directors = x.Directors,
        ScreenWriters = x.Screenwriters,
        Actors = x.Actors,
        UserScore = userScore,
        InWatchlist = userWatchlist.Contains(x.Id)
      })
      .FirstOrDefaultAsync(cancellationToken);

    return film ?? throw new FilmNotFoundException(request.Id);
  }
}