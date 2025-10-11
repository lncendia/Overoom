using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.Queries.Films;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Films;

/// <summary>
/// Обработчик запроса для поиска фильмов с фильтрацией
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class SearchFilmsQueryHandler(MongoDbContext context)
    : IRequestHandler<SearchFilmsQuery, CountResult<FilmShortDto>>
{
    /// <summary>
    /// Обрабатывает запрос на поиск фильмов с пагинацией
    /// </summary>
    /// <param name="request">Параметры поиска и фильтрации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат с краткой информацией о фильмах и общим количеством</returns>
    public async Task<CountResult<FilmShortDto>> Handle(SearchFilmsQuery request, CancellationToken cancellationToken)
    {
        // Базовый запрос к коллекции фильмов
        var baseQuery = context.Films.AsQueryable();

        // Фильтрация по поисковому запросу (в названии фильма)
        if (!string.IsNullOrEmpty(request.Query))
            baseQuery = baseQuery.Where(f =>
                f.Title.Contains(request.Query, StringComparison.CurrentCultureIgnoreCase));

        // Фильтрация по жанру
        if (!string.IsNullOrEmpty(request.Genre))
            baseQuery = baseQuery.Where(f =>
                f.Genres.Any(g => g.Contains(request.Genre, StringComparison.CurrentCultureIgnoreCase)));

        // Фильтрация по участнику (режиссер, сценарист или актер)
        if (!string.IsNullOrEmpty(request.Person))
            baseQuery = baseQuery.Where(f =>
                f.Directors.Any(d => d.Contains(request.Person, StringComparison.CurrentCultureIgnoreCase)) ||
                f.Screenwriters.Any(s => s.Contains(request.Person, StringComparison.CurrentCultureIgnoreCase)) ||
                f.Actors.Any(a => a.Name.Contains(request.Person, StringComparison.CurrentCultureIgnoreCase))
            );

        // Фильтрация по стране производства
        if (!string.IsNullOrEmpty(request.Country))
            baseQuery = baseQuery.Where(f =>
                f.Countries.Any(c => c.Contains(request.Country, StringComparison.CurrentCultureIgnoreCase)));

        // Фильтрация по типу (сериал или фильм)
        if (request.Serial.HasValue)
            baseQuery = baseQuery.Where(f => (f.Seasons != null && f.Content == null) == request.Serial.Value);

        // Фильтрация по минимальному году выпуска
        if (request.MinYear.HasValue)
            baseQuery = baseQuery.Where(f => f.Date.Year >= request.MinYear);

        // Фильтрация по максимальному году выпуска
        if (request.MaxYear.HasValue)
            baseQuery = baseQuery.Where(f => f.Date.Year <= request.MaxYear);

        // Фильтрация по плейлисту (если указан ID плейлиста)
        if (request.PlaylistId != null)
        {
            // Получаем список ID фильмов из указанного плейлиста
            var ids = await context.Playlists.AsQueryable()
                .Where(p => p.Id == request.PlaylistId)
                .Select(p => p.Films)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            // Если плейлист не найден, используем пустой список
            ids ??= [];

            // Фильтруем фильмы по списку из плейлиста
            baseQuery = baseQuery.Where(f => ids.Contains(f.Id));
        }

        // Получаем общее количество найденных фильмов
        var count = await baseQuery.CountAsync(cancellationToken);

        // Если фильмы не найдены, возвращаем пустой результат
        if (count == 0) return CountResult<FilmShortDto>.NoValues();

        // Выполняем запрос и получаем результаты в виде списка
        var films = await baseQuery
            .OrderByDescending(x => x.Date)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(x => new FilmShortDto
            {
                Id = x.Id,
                Title = x.Title,
                PosterKey = x.PosterKey,
                Year = x.Date.Year,
                RatingKp = x.RatingKp,
                RatingImdb = x.RatingImdb,
                Description = x.ShortDescription 
                              ?? x.Description.Substring(0, 150) + (x.Description.Length > 150 ? "..." : string.Empty),
                IsSerial = x.Seasons != null && x.Content == null,
                Genres = x.Genres
            })
            .ToListAsync(cancellationToken);

        // Возвращаем результат с данными и общим количеством
        return new CountResult<FilmShortDto>
        {
            List = films,
            TotalCount = count
        };
    }
}