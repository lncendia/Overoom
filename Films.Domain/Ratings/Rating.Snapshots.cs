using System.Reflection;
using Films.Domain.Ratings.Snapshots;

namespace Films.Domain.Ratings;

public partial class Rating
{
  internal static Rating FromSnapshot(RatingSnapshot snapshot)
  {
    Type type = typeof(Rating);
    ConstructorInfo? ctor = type.GetConstructor(
      BindingFlags.NonPublic | BindingFlags.Instance,
      null,
      [typeof(RatingSnapshot)],
      null);

    return (Rating)ctor!.Invoke([snapshot]);
  }

  internal RatingSnapshot GetSnapshot()
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

  // Приватный конструктор для гидратации
  // ReSharper disable once UnusedMember.Local
  private Rating(RatingSnapshot snapshot) : base(snapshot.Id)
  {
    FilmId = snapshot.FilmId;
    UserId = snapshot.UserId;
    Score = snapshot.Score;
    CreatedAt = snapshot.CreatedAt;
  }
}
