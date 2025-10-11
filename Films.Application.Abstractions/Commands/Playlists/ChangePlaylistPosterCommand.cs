using Films.Application.Abstractions.DTOs.Common;
using MediatR;

namespace Films.Application.Abstractions.Commands.Playlists;

/// <summary>
/// Команда для изменения постера плейлиста
/// </summary>
public class ChangePlaylistPosterCommand : IRequest
{
    /// <summary>
    /// Идентификатор плейлиста
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// Файл постера
    /// </summary>
    public required FileDto Poster { get; init; }
}