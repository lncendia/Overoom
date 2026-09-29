using Common.Infrastructure.Repositories.Models;

using Films.Domain.Rooms.Snapshots;

namespace Films.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель комнаты для совместного просмотра фильмов.
/// </summary>
public class RoomModel : IModel<RoomSnapshot>
{
  #region Полся и свойства

  /// <summary>
  /// Секретный код комнаты для подключения
  /// </summary>
  public string? Code { get; set; }

  /// <summary>
  /// Список идентификаторов участников комнаты
  /// </summary>
  public List<Guid> Viewers { get; set; } = [];

  /// <summary>
  /// Список идентификаторов заблокированных пользователей
  /// </summary>
  public List<Guid> BannedUsers { get; set; } = [];

  /// <summary>
  /// Идентификатор текущего фильма в комнате
  /// </summary>
  public Guid FilmId { get; set; }

  /// <summary>
  /// Идентификатор владельца комнаты
  /// </summary>
  public Guid OwnerId { get; set; }

  /// <summary>
  /// Дата и время создания комнаты
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// Дата и время изменения модели
  /// </summary>
  public DateTime ModifiedAt { get; set; }

  #endregion

  #region IModel

  /// <summary>
  /// Уникальный идентификатор комнаты
  /// </summary>
  public required Guid Id { get; init; }

  public void UpdateFromSnapshot(RoomSnapshot snapshot)
  {
    FilmId = snapshot.FilmId;
    Code = snapshot.Code;
    OwnerId = snapshot.OwnerId;
    CreatedAt = snapshot.CreatedAt;
    Viewers = [.. snapshot.Viewers];
    BannedUsers = [.. snapshot.BannedUsers];
  }

  public RoomSnapshot GetSnapshot()
  {
    return new RoomSnapshot
    {
      Id = Id,
      FilmId = FilmId,
      Code = Code,
      OwnerId = OwnerId,
      CreatedAt = CreatedAt,
      Viewers = Viewers.AsReadOnly(),
      BannedUsers = BannedUsers.AsReadOnly()
    };
  }

  #endregion
}
