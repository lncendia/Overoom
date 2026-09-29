using Common.Domain.Aggregates;

using Rooms.Domain.Rooms.Entities;
using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Domain.Rooms;

public partial class Room : ISnapshotable<Room, RoomSnapshot>
{
  /// <inheritdoc/>
  static Room ISnapshotable<Room, RoomSnapshot>.Restore(RoomSnapshot snapshot)
  {
    return new Room(snapshot);
  }

  /// <inheritdoc/>
  RoomSnapshot ISnapshotable<Room, RoomSnapshot>.ToSnapshot()
  {
    return new RoomSnapshot
    {
      Id = Id,
      FilmId = FilmId,
      IsSerial = IsSerial,
      OwnerId = Owner.Id,
      Viewers = Viewers.Values.Select(v => v.GetSnapshot()).ToDictionary(v => v.Id, v => v)
    };
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  private Room(RoomSnapshot snapshot) : base(snapshot.Id)
  {
    var viewers = snapshot.Viewers.Values
      .Select(Viewer.FromSnapshot)
      .ToDictionary(v => v.Id);

    FilmId = snapshot.FilmId;
    IsSerial = snapshot.IsSerial;
    Owner = viewers[snapshot.OwnerId];
    _viewersList = viewers;
  }
}
