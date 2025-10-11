using MediatR;

namespace Films.Application.Abstractions.Commands.Comments;

/// <summary>
/// Команда для добавления комментария к фильму
/// </summary>
public class AddCommentCommand : IRequest<Guid>
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid FilmId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Текст комментария
    /// </summary>
    public required string Text { get; init; }
}