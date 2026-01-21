using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель комнаты для работы с базой данных.
/// </summary>
public class RoomModel
{
  /// <summary>
  /// Уникальный идентификатор комнаты
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Идентификатор фильма
  /// </summary>
  public Guid FilmId { get; set; }

  /// <summary>
  /// Идентификатор владельца комнаты
  /// </summary>
  public Guid OwnerId { get; set; }

  /// <summary>
  /// Признак того, является ли контент сериалом
  /// </summary>
  public bool IsSerial { get; set; }

  /// <summary>
  /// Список пользователей, подключенных к комнате
  /// </summary>
  public List<ViewerModel> Viewers { get; set; } = [];

  /// <summary>
  /// Создаёт снапшот текущей комнаты со всеми зрителями
  /// </summary>
  public RoomSnapshot GetSnapshot() => new()
  {
    Id = Id,
    FilmId = FilmId,
    OwnerId = OwnerId,
    IsSerial = IsSerial,
    Viewers = Viewers.Select(v => v.GetSnapshot()).ToList()
  };

  /// <summary>
  /// Обновляет модель комнаты из снапшота
  /// </summary>
  public void UpdateFromSnapshot(RoomSnapshot snapshot)
  {
    FilmId = snapshot.FilmId;
    OwnerId = snapshot.OwnerId;
    IsSerial = snapshot.IsSerial;

    // Удаляем зрителей, которых больше нет в снапшоте
    Viewers.RemoveAll(viewerModel => snapshot.Viewers.All(viewer => viewerModel.Id != viewer.Id));

    // Добавляем новых зрителей, которых нет в модели
    ViewerModel[] newViewers = snapshot.Viewers
      .Where(x => Viewers.All(m => x.Id != m.Id))
      .Select(c =>
      {
        var viewerModel = new ViewerModel { Id = c.Id, UserName = null!, Settings = null! };
        viewerModel.UpdateFromSnapshot(c);
        return viewerModel;
      })
      .ToArray();

    // Обновляем существующих зрителей
    Viewers.ForEach(v => v.UpdateFromSnapshot(snapshot.Viewers.First(sv => sv.Id == v.Id)));

    // Добавляем новых зрителей в модель
    Viewers.AddRange(newViewers);
  }
}
