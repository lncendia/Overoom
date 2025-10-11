using Films.Application.Abstractions.DTOs.Playlists;
using Films.Application.Abstractions.Exceptions;
using Films.Application.Abstractions.Queries.Playlists;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Playlists;

/// <summary>
/// Обработчик запроса для получения плейлиста по идентификатору
/// </summary>
/// <param name="context">Контекст базы данных MongoDB для работы с коллекцией плейлистов</param>
public class GetPlaylistByIdQueryHandler(MongoDbContext context) : IRequestHandler<GetPlaylistByIdQuery, PlaylistDto>
{
    /// <summary>
    /// Обрабатывает запрос на получение плейлиста по ID
    /// </summary>
    /// <param name="request">Запрос, содержащий идентификатор плейлиста</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO плейлиста с указанным идентификатором</returns>
    /// <exception cref="PlaylistNotFoundException">Выбрасывается, если плейлист с указанным ID не найден</exception>
    public async Task<PlaylistDto> Handle(GetPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        // Находим первый плейлист с указанным ID или null, если не найден
        var playlist = await context.Playlists.AsQueryable()
            .Select(p => new PlaylistDto
            {
                Id = p.Id,
                Name = p.Name,
                Genres = p.Genres,
                Description = p.Description,
                PosterKey = p.PosterKey,
                Updated = p.UpdatedAt
            })
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken: cancellationToken);

        // Возвращаем найденный плейлист или выбрасываем исключение, если не найден
        return playlist ?? throw new PlaylistNotFoundException(request.Id);
    }
}