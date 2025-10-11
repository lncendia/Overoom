using MediatR;

namespace Films.Application.Abstractions.Commands.Ratings;

/// <summary>
/// Команда для установки оценки фильму пользователем
/// </summary>
public class SetRatingCommand : IRequest
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid FilmId { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; set; }
    
    /// <summary>
    /// Оценка (от 1 до 10)
    /// </summary>
    public required double Score { get; init; }
}