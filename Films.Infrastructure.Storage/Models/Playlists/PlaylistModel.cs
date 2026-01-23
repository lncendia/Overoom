using Common.Infrastructure.Repositories.Models;

using Films.Domain.Playlists.Snapshots;

namespace Films.Infrastructure.Storage.Models.Playlists;

/// <summary>
/// Модель плейлиста для работы с базой данных.
/// </summary>
public class PlaylistModel : IModel<PlaylistSnapshot>
{
  #region Поля и свойства

  /// <summary>
  /// Название плейлиста
  /// </summary>
  public string Name { get; set; } = null!;

  /// <summary>
  /// Описание плейлиста
  /// </summary>
  public string Description { get; set; } = null!;

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
  public string PosterKey { get; set; } = null!;

  #endregion

  #region IModel

  /// <summary>
  /// Уникальный идентификатор плейлиста
  /// </summary>
  public required Guid Id { get; init; }

  public void UpdateFromSnapshot(PlaylistSnapshot snapshot)
  {
    Name = snapshot.Name;
    Description = snapshot.Description;
    PosterKey = snapshot.PosterKey;
    UpdatedAt = snapshot.UpdatedAt;
    Films = snapshot.Films.ToList();
    Genres = snapshot.Genres.ToList();
  }

  public PlaylistSnapshot GetSnapshot()
  {
    return new PlaylistSnapshot
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

  #endregion
}
