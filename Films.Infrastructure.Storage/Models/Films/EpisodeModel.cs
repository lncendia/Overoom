namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель эпизода сериала для хранения в MongoDB.
/// </summary>
public class EpisodeModel
{
  /// <summary>
  /// Номер эпизода в сезоне
  /// </summary>
  public int Number { get; set; }

  /// <summary>
  /// Список версий медиаконтента для эпизода
  /// </summary>
  public List<string> Versions { get; set; } = [];
}