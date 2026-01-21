using Films.Domain.Ratings.Snapshots;

namespace Films.Infrastructure.Storage.Models.Ratings;

/// <summary>
/// Модель рейтинга для работы с базой данных.
/// </summary>
public class RatingModel
{
  /// <summary>
  /// Уникальный идентификатор рейтинга
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Идентификатор фильма, к которому относится рейтинг
  /// </summary>
  public Guid FilmId { get; set; }

  /// <summary>
  /// Идентификатор пользователя, поставившего оценку (может быть null)
  /// </summary>
  public Guid UserId { get; set; }

  /// <summary>
  /// Значение оценки (рейтинга)
  /// </summary>
  public double Score { get; set; }

  /// <summary>
  /// Дата и время выставления оценки
  /// </summary>
  public DateTime CreatedAt { get; set; }
  
  /// <summary>
  /// Дата и время изменения модели
  /// </summary>
  public DateTime ModifiedAt { get; set; }

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