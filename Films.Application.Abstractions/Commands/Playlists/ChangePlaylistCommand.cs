using MediatR;

namespace Films.Application.Abstractions.Commands.Playlists;

/// <summary>
/// Команда для изменения плейлиста
/// </summary>
public class ChangePlaylistCommand : IRequest
{
    /// <summary>
    /// Идентификатор плейлиста
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// Описание плейлиста
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Список идентификаторов фильмов в плейлисте (может быть null)
    /// </summary>
    public IReadOnlyCollection<Guid>? Films { get; init; }
}