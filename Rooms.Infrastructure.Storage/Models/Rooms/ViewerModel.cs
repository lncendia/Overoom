using Common.Domain.Rooms;
using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель зрителя для работы с базой данных.
/// </summary>
public class ViewerModel
{
  /// <summary>
  /// Уникальный идентификатор пользователя
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Имя пользователя
  /// </summary>
  public string UserName { get; set; } = null!;

  /// <summary>
  /// Ключ фото пользователя
  /// </summary>
  public string? PhotoKey { get; set; }

  /// <summary>
  /// Права пользователя на действия в комнате
  /// </summary>
  public RoomSettings Settings { get; set; } = null!;

  /// <summary>
  /// Состояние: онлайн/оффлайн
  /// </summary>
  public bool Online { get; set; }

  /// <summary>
  /// Признак полноэкранного режима
  /// </summary>
  public bool FullScreen { get; set; }

  /// <summary>
  /// Признак паузы воспроизведения
  /// </summary>
  public bool OnPause { get; set; }

  /// <summary>
  ///
  /// </summary>
  public double Speed { get; set; }

  /// <summary>
  ///
  /// </summary>
  public bool Muted { get; set; }

  /// <summary>
  /// Положение на таймлайне
  /// </summary>
  public TimeSpan TimeLine { get; set; }

  /// <summary>
  /// Номер текущего сезона (если контент — сериал)
  /// </summary>
  public int? Season { get; set; }

  /// <summary>
  /// Номер текущей серии (если контент — сериал)
  /// </summary>
  public int? Episode { get; set; }

  /// <summary>
  /// Список пользователей, подключенных к комнате
  /// </summary>
  public List<string> Tags { get; set; } = [];

  /// <summary>
  /// Список пользователей, подключенных к комнате
  /// </summary>
  public List<StatisticProperty> Statistic { get; set; } = [];

  /// <summary>
  /// Создаёт снапшот текущего состояния модели для хранения или передачи.
  /// </summary>
  public ViewerSnapshot GetSnapshot()
  {
    return new ViewerSnapshot
    {
      Id = Id,
      UserName = UserName,
      PhotoKey = PhotoKey,
      Settings = Settings,
      Online = Online,
      FullScreen = FullScreen,
      OnPause = OnPause,
      TimeLine = TimeLine,
      Season = Season,
      Episode = Episode,
      Speed = Speed,
      Muted = Muted,
      Tags = Tags.ToHashSet(),
      Statistic = Statistic.ToDictionary(s => s.Name, s => s.Value)
    };
  }

  /// <summary>
  /// Обновляет модель на основе снапшота.
  /// </summary>
  public void UpdateFromSnapshot(ViewerSnapshot snapshot)
  {
    UserName = snapshot.UserName;
    PhotoKey = snapshot.PhotoKey;
    Settings = snapshot.Settings;
    Online = snapshot.Online;
    FullScreen = snapshot.FullScreen;
    OnPause = snapshot.OnPause;
    TimeLine = snapshot.TimeLine;
    Season = snapshot.Season;
    Episode = snapshot.Episode;
    Speed = snapshot.Speed;
    Muted = snapshot.Muted;
    Tags = snapshot.Tags.ToList();

    // Удаляем отсутствующие
    Statistic.RemoveAll(s => !snapshot.Statistic.ContainsKey(s.Name));

    // Обновляем существующие
    foreach (StatisticProperty stat in Statistic)
    {
      stat.Value = snapshot.Statistic[stat.Name];
    }

    // Добавляем новые
    var existingNames = Statistic.Select(s => s.Name).ToHashSet();

    foreach ((string name, int value) in snapshot.Statistic)
    {
      if (existingNames.Contains(name)) continue;

      Statistic.Add(new StatisticProperty
      {
        Name = name,
        Value = value
      });
    }
  }
}
