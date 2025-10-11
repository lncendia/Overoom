using MediatR;

namespace Films.Application.Abstractions.Commands.Comments;

/// <summary>
/// Команда для удаления комментария
/// </summary>
public class RemoveCommentCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя, удаляющего комментарий
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Идентификатор комментария для удаления
    /// </summary>
    public required Guid CommentId { get; init; }
}