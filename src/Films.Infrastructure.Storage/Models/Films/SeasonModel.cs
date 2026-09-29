namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель сезона сериала для хранения в MongoDB.
/// </summary>
public class SeasonModel
{
  /// <summary>
  /// Номер сезона
  /// </summary>
  public int Number { get; set; }

  /// <summary>
  /// Список эпизодов сезона
  /// </summary>
  public List<EpisodeModel> Episodes { get; set; } = [];
}