using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.Queries.Profile;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Profile;

/// <summary>
/// Обработчик запроса истории просмотров пользователя
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetUserWatchlistQueryHandler(MongoDbContext context)
    : IRequestHandler<GetUserWatchlistQuery, IReadOnlyList<FilmShortDto>>
{
    /// <summary>
    /// Обрабатывает запрос на получение истории просмотров пользователя
    /// </summary>
    /// <param name="request">Запрос с ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список DTO фильмов из истории пользователя</returns>
    public async Task<IReadOnlyList<FilmShortDto>> Handle(GetUserWatchlistQuery request, CancellationToken cancellationToken)
    {
        // Выполняем запрос и получаем результаты в виде списка
        return await context.Users.AsQueryable()
            .Where(u => u.Id == request.Id)
            .SelectMany(u => u.Watchlist)
            .GroupJoin(
                context.Films.AsQueryable(),
                note => note.FilmId,
                film => film.Id,
                (note, films) => new
                {
                    note.Date,
                    Film = films.First(),
                }
            )
            .OrderByDescending(x => x.Date)
            .Select(x => new FilmShortDto
            {
                Id = x.Film.Id,
                Title = x.Film.Title,
                PosterKey = x.Film.PosterKey,
                Year = x.Film.Date.Year,
                RatingKp = x.Film.RatingKp,
                RatingImdb = x.Film.RatingImdb,
                Description = x.Film.ShortDescription
                              ?? x.Film.Description.Substring(0, 150) +
                              (x.Film.Description.Length > 150 ? "..." : string.Empty),
                IsSerial = x.Film.Seasons != null && x.Film.Content == null,
                Genres = x.Film.Genres
            })
            .ToListAsync(cancellationToken);
    }
}