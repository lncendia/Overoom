namespace Films.Domain.Comments.Snapshots;

public class CommentSnapshot
{
    public required Guid Id { get; init; }
    public required Guid FilmId { get; init; }
    public required Guid UserId { get; init; }
    public required string Text { get; init; }
    public required DateTime CreatedAt { get; init; }
}

