using MediatR;

namespace Films.Application.Abstractions.Commands.Playlists;

/// <summary>
/// Команда для удаления плейлиста
/// </summary>
public class DeletePlaylistCommand : IRequest
{
    /// <summary>
    /// Идентификатор плейлиста
    /// </summary>
    public required Guid Id { get; init; }
}