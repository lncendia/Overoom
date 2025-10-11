using MediatR;

namespace Films.Application.Abstractions.Commands.Profile;

/// <summary>
/// Команда для добавления/удаления фильма из списка ожидания пользователя
/// </summary>
public class ToggleWatchListCommand : IRequest
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid FilmId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }
}