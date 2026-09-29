using Common.Domain.Rooms;
using Common.Infrastructure.Repositories.Models;

using Films.Domain.Users.Snapshots;
using Films.Domain.Users.ValueObjects;

using MongoDB.Bson.Serialization.Attributes;

namespace Films.Infrastructure.Storage.Models.Users;

/// <summary>
/// Модель пользователя для работы с базой данных.
/// </summary>
[BsonIgnoreExtraElements]
public class UserModel : IModel<UserSnapshot>
{
  #region Поля и свойства

  /// <summary>
  /// Имя пользователя
  /// </summary>
  public string Username { get; set; } = null!;

  /// <summary>
  /// Ссылка на фото пользователя
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
  /// Количество оценённых пользователем фильмов по каждому жанру.
  /// </summary>
  /// <remarks>
  /// Денормализованная статистика для профиля. Не входит в снапшот агрегата и обновляется
  /// только атомарным $inc из <see cref="EventHandlers.RatingCreatedStatisticsEventHandler"/>, поэтому трекер её не перезаписывает.
  /// </remarks>
  public Dictionary<string, int> GenreCounts { get; set; } = [];

  /// <summary>
  /// Настройка комнат
  /// </summary>
  public RoomSettings RoomSettings { get; set; } = null!;

  /// <summary>
  /// Дата и время изменения модели
  /// </summary>
  public DateTime ModifiedAt { get; set; }

  #endregion

  #region IModel

  /// <summary>
  /// Уникальный идентификатор пользователя
  /// </summary>
  public Guid Id { get; init; }

  public void UpdateFromSnapshot(UserSnapshot snapshot)
  {
    Username = snapshot.Username;
    PhotoKey = snapshot.PhotoKey;
    RoomSettings = snapshot.RoomSettings;
    Watchlist = [.. snapshot.Watchlist];
    History = [.. snapshot.History];
  }

  public UserSnapshot GetSnapshot()
  {
    return new UserSnapshot
    {
      Id = Id,
      Username = Username,
      PhotoKey = PhotoKey,
      RoomSettings = RoomSettings,
      Watchlist = Watchlist,
      History = History
    };
  }

  #endregion
}
