using Films.Domain.Playlists.Snapshots;

namespace Films.Infrastructure.Storage.Models.Playlists;

/// <summary>
/// Модель плейлиста для работы с базой данных.
/// </summary>
public class PlaylistModel
{
  /// <summary>
  /// Уникальный идентификатор плейлиста
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Название плейлиста (максимальная длина - 200 символов)
  /// </summary>
  public required string Name { get; set; }

  /// <summary>
  /// Описание плейлиста (максимальная длина - 500 символов)
  /// </summary>
  public required string Description { get; set; }

  /// <summary>
  /// Список идентификаторов фильмов в плейлисте
  /// </summary>
  public List<Guid> Films { get; set; } = [];

  /// <summary>
  /// Список жанров плейлиста
  /// </summary>
  public List<string> Genres { get; set; } = [];

  /// <summary>
  /// Дата и время последнего обновления плейлиста
  /// </summary>
  public DateTime UpdatedAt { get; set; }

  /// <summary>
  /// Ссылка на постер плейлиста
  /// </summary>
  public required string PosterKey { get; set; }

  public void UpdateFromSnapshot(PlaylistSnapshot snapshot)
  {
    Name = snapshot.Name;
    Description = snapshot.Description;
    PosterKey = snapshot.PosterKey;
    UpdatedAt = snapshot.UpdatedAt;
    Films = snapshot.Films.ToList();
    Genres = snapshot.Genres.ToList();
  }

  public PlaylistSnapshot GetSnapshot() => new()
  {
    Id = Id,
    Name = Name,
    Description = Description,
    PosterKey = PosterKey,
    UpdatedAt = UpdatedAt,
    Films = Films,
    Genres = Genres
  };
}