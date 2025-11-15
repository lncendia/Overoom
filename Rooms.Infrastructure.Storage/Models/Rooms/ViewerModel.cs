using Common.Domain.Rooms;
using MongoDB.Driver;
using MongoTracker.Entities;
using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель зрителя для работы с базой данных.
/// Наследуется от абстрактного класса <see cref="UpdatedValueObject{TParent}"/>,
/// который предоставляет возможности отслеживания изменений и управления состоянием вложенной сущности.
/// </summary>
public class ViewerModel : UpdatedValueObject<RoomModel>
{
    private TrackedCollection<string, RoomModel> _tags = new();
    private TrackedValueObjectCollection<StatisticProperty, RoomModel> _statistic = new();

    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Имя пользователя (максимум 40 символов)
    /// </summary>
    public string UserName
    {
        get;
        set => field = TrackChange(nameof(UserName), field, value)!;
    } = null!;

    /// <summary>
    /// Ключ фото пользователя (может быть null)
    /// </summary>
    public string? PhotoKey
    {
        get;
        set => field = TrackChange(nameof(PhotoKey), field, value);
    }

    /// <summary>
    /// Права пользователя на действия в комнате
    /// </summary>
    public RoomSettings Settings
    {
        get;
        set => field = TrackChange(nameof(Settings), field, value)!;
    } = null!;

    /// <summary>
    /// Состояние: онлайн/оффлайн
    /// </summary>
    public bool Online
    {
        get;
        set => field = TrackStructChange(nameof(Online), field, value);
    }

    /// <summary>
    /// Признак полноэкранного режима
    /// </summary>
    public bool FullScreen
    {
        get;
        set => field = TrackStructChange(nameof(FullScreen), field, value);
    }

    /// <summary>
    /// Признак паузы воспроизведения
    /// </summary>
    public bool OnPause
    {
        get;
        set => field = TrackStructChange(nameof(OnPause), field, value);
    }

    /// <summary>
    ///
    /// </summary>
    public double Speed
    {
        get;
        set => field = TrackStructChange(nameof(Speed), field, value);
    }

    /// <summary>
    ///
    /// </summary>
    public bool Muted
    {
        get;
        set => field = TrackStructChange(nameof(Muted), field, value);
    }

    /// <summary>
    /// Положение на таймлайне
    /// </summary>
    public TimeSpan TimeLine
    {
        get;
        set => field = TrackStructChange(nameof(TimeLine), field, value);
    }

    /// <summary>
    /// Номер текущего сезона (если контент — сериал)
    /// </summary>
    public int? Season
    {
        get;
        set => field = TrackStructChange(nameof(Season), field, value);
    }

    /// <summary>
    /// Номер текущей серии (если контент — сериал)
    /// </summary>
    public int? Episode
    {
        get;
        set => field = TrackStructChange(nameof(Episode), field, value);
    }

    /// <summary>
    /// Список пользователей, подключенных к комнате
    /// </summary>
    public List<string> Tags
    {
        get => _tags.Collection;
        set => _tags = TrackCollection(nameof(Tags), _tags, value)!;
    }

    /// <summary>
    /// Список пользователей, подключенных к комнате
    /// </summary>
    public List<StatisticProperty> Statistic
    {
        get => _statistic.Collection;
        set => _statistic = TrackValueObjectCollection(nameof(Statistic), _statistic, value)!;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает определение обновления MongoDB путем объединения изменений всех отслеживаемых свойств сущности.
    /// </summary>
    public override UpdateDefinition<RoomModel>? GetUpdateDefinition(string? parentPropertyName, string propertyName,
        IReadOnlyCollection<string> blockedParentPropertyNames)
    {
        // Если имя родительского свойства содержится в blockedParentPropertyNames,
        // возвращает null, так как это свойство не должно обновляться, и этот объект будет записан целиком.
        if (blockedParentPropertyNames.Contains(propertyName)) return null;

        // Получить определение обновления для списка _footnotes, если он был изменен.
        var tagsDefinition =
            _tags.GetUpdateDefinition(Combine(parentPropertyName, propertyName), nameof(Tags), AddedValueObjects);
        var statisticDefinition = _statistic.GetUpdateDefinition(Combine(parentPropertyName, propertyName),
            nameof(Statistic), AddedValueObjects);

        // Получить базовое определение обновления из родительского класса.
        var baseDefinition = base.GetUpdateDefinition(parentPropertyName, propertyName, blockedParentPropertyNames);

        // Объединить все определения обновления в одно и вернуть результат.
        return Combine(tagsDefinition, statisticDefinition, baseDefinition);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает текущее состояние сущности путем объединения состояний всех отслеживаемых свойств.
    /// </summary>
    public override bool IsModified => base.IsModified || _tags.IsModified || _statistic.IsModified;

    /// <inheritdoc/>
    /// <summary>
    /// Очищает все изменения в сущности и всех ее отслеживаемых свойствах.
    /// </summary>
    public override void ClearChanges()
    {
        // Очистить изменения в списке footnotes, если он существует.
        _tags.ClearChanges();
        _statistic.ClearChanges();

        // Очистить изменения в базовой сущности.
        base.ClearChanges();
    }

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
        var newParameters = snapshot.Statistic
            .Where(x => Statistic.All(m => x.Key != m.Name))
            .Select(c => new StatisticProperty { Name = c.Key, Value = c.Value })
            .ToArray();

        // Обновляем существующие параметры
        Statistic.ForEach(v => v.Value = snapshot.Statistic[v.Name]);

        // Добавляем новые параметры в модель
        Statistic.AddRange(newParameters);
    }
}