using Films.Application.Abstractions.DTOs.Playlists;
using MediatR;

namespace Films.Application.Abstractions.Queries.Playlists;

/// <summary>
/// Запрос для получения информации о плейлисте по идентификатору
/// </summary>
public class GetPlaylistByIdQuery : IRequest<PlaylistDto>
{
    /// <summary>
    /// Идентификатор плейлиста
    /// </summary>
    public required Guid Id { get; init; }
}