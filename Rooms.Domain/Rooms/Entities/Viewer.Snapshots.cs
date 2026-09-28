using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Domain.Rooms.Entities;

public partial class Viewer
{
  /// <summary>
  /// Восстанавливает сущность из снапшота.
  /// </summary>
  internal static Viewer FromSnapshot(ViewerSnapshot snapshot)
  {
    return new Viewer(snapshot);
  }

  internal ViewerSnapshot GetSnapshot()
  {
    return new ViewerSnapshot
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
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
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
    _tagsSet = [.. snapshot.Tags];
    _statisticDictionary = snapshot.Statistic.ToDictionary(p => p.Key, p => p.Value);
    Settings = snapshot.Settings;
  }
}