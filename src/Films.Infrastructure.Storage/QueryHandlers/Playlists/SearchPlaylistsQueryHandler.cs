using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Playlists;
using Films.Application.Abstractions.Queries.Playlists;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Playlists;

using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Infrastructure.Storage.QueryHandlers.Playlists;

/// <summary>
/// Обработчик запроса для поиска плейлистов
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class SearchPlaylistsQueryHandler(MongoDbContext context)
  : IRequestHandler<SearchPlaylistsQuery, CountResult<PlaylistDto>>
{
  /// <summary>
  /// Обрабатывает запрос на поиск плейлистов
  /// </summary>
  /// <param name="request">Параметры запроса</param>
  /// <param name="cancellationToken">Токен отмены</param>
  /// <returns>Результат с найденными плейлистами и общим количеством</returns>
  public async Task<CountResult<PlaylistDto>> Handle(SearchPlaylistsQuery request, CancellationToken cancellationToken)
  {
    IQueryable<PlaylistModel>? baseQuery = context.Playlists.AsQueryable();

    if (!string.IsNullOrEmpty(request.Query))
      baseQuery =
        baseQuery.Where(x => x.Name.Contains(request.Query, StringComparison.CurrentCultureIgnoreCase));

    if (!string.IsNullOrEmpty(request.Genre))
      baseQuery = baseQuery.Where(x =>
        x.Genres.Any(g => g.Contains(request.Genre, StringComparison.CurrentCultureIgnoreCase)));

    if (request.FilmId.HasValue)
      baseQuery = baseQuery.Where(x => x.Films.Contains(request.FilmId.Value));

    int count = await baseQuery.CountAsync(cancellationToken: cancellationToken);
    if (count == 0) return CountResult<PlaylistDto>.NoValues();

    List<PlaylistDto>? list = await baseQuery
      .Skip(request.Skip)
      .Take(request.Take)
      .Select(p => new PlaylistDto
      {
        Id = p.Id,
        Name = p.Name,
        Genres = p.Genres,
        Description = p.Description,
        PosterKey = p.PosterKey,
        Updated = p.UpdatedAt
      })
      .ToListAsync(cancellationToken: cancellationToken);

    return new CountResult<PlaylistDto>
    {
      List = list,
      TotalCount = count
    };
  }
}