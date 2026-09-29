using Common.Domain.Aggregates;
using Films.Domain.Comments.Snapshots;

namespace Films.Domain.Comments;

public partial class Comment : ISnapshotable<Comment, CommentSnapshot>
{
  /// <inheritdoc/>
  static Comment ISnapshotable<Comment, CommentSnapshot>.Restore(CommentSnapshot snapshot)
  {
    return new Comment(snapshot);
  }

  /// <inheritdoc/>
  CommentSnapshot ISnapshotable<Comment, CommentSnapshot>.ToSnapshot()
  {
    return new CommentSnapshot
    {
      Id = Id,
      FilmId = FilmId,
      UserId = UserId,
      Text = Text,
      CreatedAt = CreatedAt
    };
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  private Comment(CommentSnapshot snapshot) : base(snapshot.Id)
  {
    FilmId = snapshot.FilmId;
    UserId = snapshot.UserId;
    Text = snapshot.Text;
    CreatedAt = snapshot.CreatedAt;
  }
}
