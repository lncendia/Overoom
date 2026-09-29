using Common.Domain.Events;

namespace Films.Domain.Ratings.Events;

/// <summary>
/// Событие, представляющее изменение пользователем оценки фильма
/// </summary>
public class RatingScoreChangedEvent(Rating rating, double oldScore) : DomainEvent
{
  /// <summary>
  /// Изменённая оценка
  /// </summary>
  public Rating Rating { get; } = rating;

  /// <summary>
  /// Значение оценки до изменения
  /// </summary>
  public double OldScore { get; } = oldScore;
}
