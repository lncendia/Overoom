namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель медиаконтента для хранения в MongoDB.
/// </summary>
public class MediaContentModel
{
  /// <summary>
  /// Список версий медиаконтента
  /// </summary>
  public List<string> Versions { get; set; } = [];
}