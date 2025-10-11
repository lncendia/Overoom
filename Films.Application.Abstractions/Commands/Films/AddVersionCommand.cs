using MediatR;

namespace Films.Application.Abstractions.Commands.Films;

/// <summary>
/// Команда для добавления новой версии медиафайла фильма
/// </summary>
public class AddVersionCommand : IRequest
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid FilmId { get; init; }
    
    /// <summary>
    /// Название версии медиафайла
    /// </summary>
    public required string Version { get; init; }
    
    /// <summary>
    /// Номер сезона (для сериалов, может быть null)
    /// </summary>
    public int? Season { get; init; }
    
    /// <summary>
    /// Номер эпизода (для сериалов, может быть null)
    /// </summary>
    public int? Episode { get; init; }
}