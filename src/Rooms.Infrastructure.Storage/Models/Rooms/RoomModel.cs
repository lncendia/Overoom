using Common.Infrastructure.Repositories.Models;

using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель комнаты для работы с базой данных.
/// </summary>
public class RoomModel : IModel<RoomSnapshot>
{
  #region Поля и свойства

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

  #endregion

  #region IModel

  /// <summary>
  /// Уникальный идентификатор комнаты
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Создаёт снапшот текущей комнаты со всеми зрителями
  /// </summary>
  public RoomSnapshot GetSnapshot()
  {
    return new RoomSnapshot
    {
      Id = Id,
      FilmId = FilmId,
      OwnerId = OwnerId,
      IsSerial = IsSerial,
      Viewers = Viewers.Select(v => v.GetSnapshot()).ToDictionary(s => s.Id, s => s)
    };
  }

  /// <summary>
  /// Обновляет модель комнаты из снапшота
  /// </summary>
  public void UpdateFromSnapshot(RoomSnapshot snapshot)
  {
    FilmId = snapshot.FilmId;
    OwnerId = snapshot.OwnerId;
    IsSerial = snapshot.IsSerial;
    Viewers.RemoveAll(v => !snapshot.Viewers.ContainsKey(v.Id));

    foreach (ViewerModel viewer in Viewers)
    {
      viewer.UpdateFromSnapshot(snapshot.Viewers[viewer.Id]);
    }

    var existingIds = Viewers.Select(v => v.Id).ToHashSet();

    foreach (ViewerSnapshot snapshotViewer in snapshot.Viewers.Values)
    {
      if (existingIds.Contains(snapshotViewer.Id)) continue;

      var viewerModel = new ViewerModel { Id = snapshotViewer.Id };
      viewerModel.UpdateFromSnapshot(snapshotViewer);
      Viewers.Add(viewerModel);
    }
  }

  #endregion
}
