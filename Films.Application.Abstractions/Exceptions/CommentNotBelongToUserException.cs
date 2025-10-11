namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда комментарий не принадлежит пользователю.
/// </summary>
public class CommentNotBelongToUserException : Exception
{
    /// <summary>
    /// Идентификатор пользователя, который пытался выполнить действие.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Идентификатор комментария, который не принадлежит пользователю.
    /// </summary>
    public Guid CommentId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="commentId">Идентификатор комментария.</param>
    public CommentNotBelongToUserException(Guid userId, Guid commentId) 
        : base($"Comment with ID {commentId} does not belong to user with ID {userId}.")
    {
        UserId = userId;
        CommentId = commentId;
    }
}