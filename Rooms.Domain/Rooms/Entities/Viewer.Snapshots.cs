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
        _userName = snapshot.UserName;
        _photoKey = snapshot.PhotoKey;
        _online = snapshot.Online;
        _fullScreen = snapshot.FullScreen;
        _onPause = snapshot.OnPause;
        _timeLine = snapshot.TimeLine;
        _season = snapshot.Season;
        _episode = snapshot.Episode;
        _speed = snapshot.Speed;
        _muted = snapshot.Muted;
        _tagsSet = snapshot.Tags.ToHashSet();
        _statisticDictionary = snapshot.Statistic.ToDictionary(p => p.Key, p => p.Value);
        _settings = snapshot.Settings;
    }
}