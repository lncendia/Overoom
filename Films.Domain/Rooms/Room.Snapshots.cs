using Common.Domain.Aggregates;
using Films.Domain.Rooms.Snapshots;

namespace Films.Domain.Rooms;

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
      Code = Code,
      OwnerId = OwnerId,
      CreatedAt = CreatedAt,
      Viewers = [.. _viewers],
      BannedUsers = [.. _bannedUsers]
    };
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  private Room(RoomSnapshot snapshot) : base(snapshot.Id)
  {
    FilmId = snapshot.FilmId;
    Code = snapshot.Code;
    OwnerId = snapshot.OwnerId;
    CreatedAt = snapshot.CreatedAt;
    _viewers = [.. snapshot.Viewers];
    _bannedUsers = [.. snapshot.BannedUsers];
  }
}
