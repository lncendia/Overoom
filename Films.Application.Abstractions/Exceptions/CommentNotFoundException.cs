namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда не удается найти комментарий.
/// </summary>
public class CommentNotFoundException : Exception
{
    /// <summary>
    /// Идентификатор комментария, который не был найден.
    /// </summary>
    public Guid CommentId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="commentId">Идентификатор комментария.</param>
    public CommentNotFoundException(Guid commentId) : base($"Comment with ID {commentId} not found.")
    {
        CommentId = commentId;
    }
}