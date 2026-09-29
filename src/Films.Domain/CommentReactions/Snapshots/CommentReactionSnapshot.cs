namespace Films.Domain.CommentReactions.Snapshots;

public class CommentReactionSnapshot
{
  public required Guid Id { get; init; }
  public required Guid CommentId { get; init; }
  public required Guid UserId { get; init; }
  public required string Reaction { get; init; }
  public required DateTime CreatedAt { get; init; }
}
