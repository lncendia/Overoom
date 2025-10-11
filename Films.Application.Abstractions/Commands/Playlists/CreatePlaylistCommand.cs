using MediatR;

namespace Films.Application.Abstractions.Commands.Playlists;

/// <summary>
/// Команда для создания нового плейлиста
/// </summary>
public class CreatePlaylistCommand : IRequest<Guid>
{
    /// <summary>
    /// Название плейлиста
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Описание плейлиста
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// Список идентификаторов фильмов в плейлисте
    /// </summary>
    public required IReadOnlyCollection<Guid> Films { get; init; }
}