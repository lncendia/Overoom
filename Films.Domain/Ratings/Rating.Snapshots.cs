using System.Reflection;
using Films.Domain.Ratings.Snapshots;

namespace Films.Domain.Ratings;

public partial class Rating
{
    internal static Rating FromSnapshot(RatingSnapshot snapshot)
    {
        var type = typeof(Rating);
        var ctor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(RatingSnapshot)],
            null);

        return (Rating)ctor!.Invoke([snapshot]);
    }

    internal RatingSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        UserId = UserId,
        Score = Score,
        CreatedAt = CreatedAt
    };

    // Приватный конструктор для гидратации
    // ReSharper disable once UnusedMember.Local
    private Rating(RatingSnapshot snapshot) : base(snapshot.Id)
    {
        FilmId = snapshot.FilmId;
        UserId = snapshot.UserId;
        _score = snapshot.Score;
        CreatedAt = snapshot.CreatedAt;
    }
}