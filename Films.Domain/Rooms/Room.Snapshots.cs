using System.Reflection;
using Films.Domain.Rooms.Snapshots;

namespace Films.Domain.Rooms;

public partial class Room
{
    internal static Room FromSnapshot(RoomSnapshot snapshot)
    {
        var type = typeof(Room);
        var ctor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(RoomSnapshot)],
            null);

        return (Room)ctor!.Invoke([snapshot]);
    }

    internal RoomSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        Code = Code,
        OwnerId = OwnerId,
        CreatedAt = CreatedAt,
        Viewers = _viewers.ToList(),
        BannedUsers = _bannedUsers.ToList()
    };

    // Приватный конструктор для гидратации
    // ReSharper disable once UnusedMember.Local
    private Room(RoomSnapshot snapshot) : base(snapshot.Id)
    {
        FilmId = snapshot.FilmId;
        Code = snapshot.Code;
        OwnerId = snapshot.OwnerId;
        CreatedAt = snapshot.CreatedAt;
        _viewers = snapshot.Viewers.ToHashSet();
        _bannedUsers = snapshot.BannedUsers.ToHashSet();
    }
}