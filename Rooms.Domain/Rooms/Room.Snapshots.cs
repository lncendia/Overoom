using System.Reflection;
using Rooms.Domain.Rooms.Entities;
using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Domain.Rooms;

public partial class Room
{
    /// <summary>
    /// Воссоздаёт агрегат Message из снапшота.
    /// </summary>
    internal static Room FromSnapshot(RoomSnapshot snapshot)
    {
        // Получаем тип Room
        var filmType = typeof(Room);
    
        // Получаем внутренний конструктор, который принимает RoomSnapshot
        var constructor = filmType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(RoomSnapshot)],
            null);
    
        // Вызываем конструктор и возвращаем результат
        return (Room)constructor!.Invoke([snapshot]);
    }
    
    internal RoomSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        IsSerial = IsSerial,
        OwnerId = Owner.Id,
        Viewers = Viewers.Values.Select(v => v.GetSnapshot()).ToArray()
    };

    /// <summary>
    /// Внутренний конструктор для гидратации из инфраструктуры
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Room(RoomSnapshot snapshot) : base(snapshot.Id)
    {
        var viewers = snapshot.Viewers
            .Select(Viewer.FromSnapshot)
            .ToDictionary(v => v.Id);
        
        FilmId = snapshot.FilmId;
        IsSerial = snapshot.IsSerial;
        Owner = viewers[snapshot.OwnerId];
        _viewersList = viewers;
    }
}