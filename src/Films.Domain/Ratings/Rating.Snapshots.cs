using Common.Domain.Aggregates;
using Films.Domain.Ratings.Snapshots;

namespace Films.Domain.Ratings;

public partial class Rating : ISnapshotable<Rating, RatingSnapshot>
{
  /// <inheritdoc/>
  static Rating ISnapshotable<Rating, RatingSnapshot>.Restore(RatingSnapshot snapshot)
  {
    return new Rating(snapshot);
  }

  /// <inheritdoc/>
  RatingSnapshot ISnapshotable<Rating, RatingSnapshot>.ToSnapshot()
  {
    return new RatingSnapshot
    {
      Id = Id,
      FilmId = FilmId,
      UserId = UserId,
      Score = Score,
      CreatedAt = CreatedAt
    };
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  private Rating(RatingSnapshot snapshot) : base(snapshot.Id)
  {
    FilmId = snapshot.FilmId;
    UserId = snapshot.UserId;
    Score = snapshot.Score;
    CreatedAt = snapshot.CreatedAt;
  }
}
