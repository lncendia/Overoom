using Films.Domain.Ratings.Snapshots;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Ratings;

/// <summary>
/// Модель рейтинга для работы с базой данных.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
public class RatingModel : VersionedUpdatedEntity<RatingModel>
{
    /// <summary>
    /// Уникальный идентификатор рейтинга
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор фильма, к которому относится рейтинг
    /// </summary>
    public Guid FilmId
    {
        get;
        set => field = TrackStructChange(nameof(FilmId), field, value);
    }

    /// <summary>
    /// Идентификатор пользователя, поставившего оценку (может быть null)
    /// </summary>
    public Guid UserId
    {
        get;
        set => field = TrackStructChange(nameof(UserId), field, value);
    }

    /// <summary>
    /// Значение оценки (рейтинга)
    /// </summary>
    public double Score
    {
        get;
        set => field = TrackStructChange(nameof(Score), field, value);
    }

    /// <summary>
    /// Дата и время выставления оценки
    /// </summary>
    public DateTime CreatedAt
    {
        get;
        set => field = TrackStructChange(nameof(CreatedAt), field, value);
    }

    public void UpdateFromSnapshot(RatingSnapshot snapshot)
    {
        FilmId = snapshot.FilmId;
        UserId = snapshot.UserId;
        Score = snapshot.Score;
        CreatedAt = snapshot.CreatedAt;
    }

    public RatingSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        UserId = UserId,
        Score = Score,
        CreatedAt = CreatedAt
    };
}