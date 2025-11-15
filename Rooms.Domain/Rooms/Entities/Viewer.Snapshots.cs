using System.Reflection;
using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Domain.Rooms.Entities;

public partial class Viewer
{
    /// <summary>
    /// Воссоздаёт агрегат Viewer из снапшота.
    /// </summary>
    internal static Viewer FromSnapshot(ViewerSnapshot snapshot)
    {
        // Получаем тип Viewer
        var filmType = typeof(Viewer);
    
        // Получаем внутренний конструктор, который принимает RoomSnapshot
        var constructor = filmType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(ViewerSnapshot)],
            null);
    
        // Вызываем конструктор и возвращаем результат
        return (Viewer)constructor!.Invoke([snapshot]);
    }

    internal ViewerSnapshot GetSnapshot() => new()
    {
        Id = Id,
        UserName = UserName,
        PhotoKey = PhotoKey,
        Online = Online,
        FullScreen = FullScreen,
        OnPause = OnPause,
        TimeLine = TimeLine,
        Season = Season,
        Episode = Episode,
        Speed = Speed,
        Muted = Muted,
        Tags = Tags,
        Statistic = Statistic,
        Settings = Settings
    };

    // приватный/internal конструктор
    // ReSharper disable once UnusedMember.Local
    private Viewer(ViewerSnapshot snapshot) : this(snapshot.Id)
    {
        UserName = snapshot.UserName;
        PhotoKey = snapshot.PhotoKey;
        Online = snapshot.Online;
        FullScreen = snapshot.FullScreen;
        OnPause = snapshot.OnPause;
        TimeLine = snapshot.TimeLine;
        Season = snapshot.Season;
        Episode = snapshot.Episode;
        Speed = snapshot.Speed;
        Muted = snapshot.Muted;
        _tagsSet = snapshot.Tags.ToHashSet();
        _statisticDictionary = snapshot.Statistic.ToDictionary(p => p.Key, p => p.Value);
        Settings = snapshot.Settings;
    }
}