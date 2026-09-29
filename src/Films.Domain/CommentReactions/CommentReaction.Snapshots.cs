using Common.Domain.Aggregates;
using Films.Domain.CommentReactions.Snapshots;

namespace Films.Domain.CommentReactions;

public partial class CommentReaction : ISnapshotable<CommentReaction, CommentReactionSnapshot>
{
  /// <inheritdoc/>
  static CommentReaction ISnapshotable<CommentReaction, CommentReactionSnapshot>.Restore(
    CommentReactionSnapshot snapshot)
  {
    return new CommentReaction(snapshot);
  }

  /// <inheritdoc/>
  CommentReactionSnapshot ISnapshotable<CommentReaction, CommentReactionSnapshot>.ToSnapshot()
  {
    return new CommentReactionSnapshot
    {
      Id = Id,
      CommentId = CommentId,
      UserId = UserId,
      Reaction = Reaction,
      CreatedAt = CreatedAt
    };
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  private CommentReaction(CommentReactionSnapshot snapshot) : base(snapshot.Id)
  {
    CommentId = snapshot.CommentId;
    UserId = snapshot.UserId;
    Reaction = snapshot.Reaction;
    CreatedAt = snapshot.CreatedAt;
  }
}
