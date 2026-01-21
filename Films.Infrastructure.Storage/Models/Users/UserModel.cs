using Common.Domain.Rooms;
using Films.Domain.Users.Snapshots;
using Films.Domain.Users.ValueObjects;

namespace Films.Infrastructure.Storage.Models.Users;

/// <summary>
/// Модель пользователя для работы с базой данных.
/// </summary>
public class UserModel
{
  /// <summary>
  /// Уникальный идентификатор пользователя
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Имя пользователя
  /// </summary>
  public required string Username { get; set; }

  /// <summary>
  /// Ссылка на фото пользователя (может быть null)
  /// </summary>
  public string? PhotoKey { get; set; }

  /// <summary>
  /// Список фильмов в "хочу посмотреть" пользователя
  /// </summary>
  public List<FilmNote> Watchlist { get; set; } = [];

  /// <summary>
  /// История просмотров пользователя
  /// </summary>
  public List<FilmNote> History { get; set; } = [];

  /// <summary>
  /// Список предпочитаемых жанров пользователя
  /// </summary>
  public List<string> Genres { get; set; } = [];

  /// <summary>
  /// Настройка комнат
  /// </summary>
  public required RoomSettings RoomSettings { get; set; }
  
  /// <summary>
  /// Дата и время изменения модели
  /// </summary>
  public DateTime ModifiedAt { get; set; }

  public void UpdateFromSnapshot(UserSnapshot snapshot)
  {
    Username = snapshot.Username;
    PhotoKey = snapshot.PhotoKey;
    RoomSettings = snapshot.RoomSettings;
    Watchlist = snapshot.Watchlist.ToList();
    History = snapshot.History.ToList();
    Genres = snapshot.Genres.ToList();
  }

  public UserSnapshot GetSnapshot() => new()
  {
    Id = Id,
    Username = Username,
    PhotoKey = PhotoKey,
    RoomSettings = RoomSettings,
    Watchlist = Watchlist,
    History = History,
    Genres = Genres
  };
}