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
  /// Имя пользователя (максимум 40 символов)
  /// </summary>
  public required string UserName { get; set; }

  /// <summary>
  /// Ключ фото пользователя (может быть null)
  /// </summary>
  public string? PhotoKey { get; set; }

  /// <summary>
  /// Права пользователя на действия в комнате
  /// </summary>
  public required RoomSettings Settings { get; set; }

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
  public ViewerSnapshot GetSnapshot() => new()
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

  /// <summary>
  /// Обновляет модель на основе снапшота.
  /// Поля обновляются с отслеживанием изменений через TrackChange/TrackStructChange/TrackCollection.
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

    // Удаляем статистику, которой больше нет в снапшоте
    Statistic.RemoveAll(statisticModel =>
      snapshot.Statistic.All(statistic => statisticModel.Name != statistic.Key));

    // Добавляем новые параметры статистики, которых нет в модели
    StatisticProperty[] newParameters = snapshot.Statistic
      .Where(x => Statistic.All(m => x.Key != m.Name))
      .Select(c => new StatisticProperty { Name = c.Key, Value = c.Value })
      .ToArray();

    // Обновляем существующие параметры
    Statistic.ForEach(v => v.Value = snapshot.Statistic[v.Name]);

    // Добавляем новые параметры в модель
    Statistic.AddRange(newParameters);
  }
}