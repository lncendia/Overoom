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
    private Guid _filmId;
    private Guid _userId;
    private double _score;
    private DateTime _createdAt;

    /// <summary>
    /// Уникальный идентификатор рейтинга
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор фильма, к которому относится рейтинг
    /// </summary>
    public Guid FilmId
    {
        get => _filmId;
        set => _filmId = TrackStructChange(nameof(FilmId), _filmId, value);
    }

    /// <summary>
    /// Идентификатор пользователя, поставившего оценку (может быть null)
    /// </summary>
    public Guid UserId
    {
        get => _userId;
        set => _userId = TrackStructChange(nameof(UserId), _userId, value);
    }

    /// <summary>
    /// Значение оценки (рейтинга)
    /// </summary>
    public double Score
    {
        get => _score;
        set => _score = TrackStructChange(nameof(Score), _score, value);
    }

    /// <summary>
    /// Дата и время выставления оценки
    /// </summary>
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => _createdAt = TrackStructChange(nameof(CreatedAt), _createdAt, value);
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