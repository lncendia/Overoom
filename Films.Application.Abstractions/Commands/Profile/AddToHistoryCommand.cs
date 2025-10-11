using MediatR;

namespace Films.Application.Abstractions.Commands.Profile;

/// <summary>
/// Команда для добавления фильма в историю просмотров пользователя
/// </summary>
public class AddToHistoryCommand : IRequest
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