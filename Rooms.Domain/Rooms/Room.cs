using Common.Domain.Aggregates;
using Common.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;
using Rooms.Domain.Rooms.Events;
using Rooms.Domain.Rooms.Exceptions;

namespace Rooms.Domain.Rooms;

/// <summary>
/// Представляет комнату совместного просмотра фильма или сериала.
/// Содержит участников, настройки воспроизведения и методы взаимодействия между зрителями.
/// </summary>
/// <remarks>
/// Класс реализует логику синхронизации просмотра между участниками,
/// управление состоянием комнаты и генерацию соответствующих доменных событий.
/// </remarks>
public partial class Room : AggregateRoot
{
    private readonly Dictionary<Guid, Viewer> _viewersList = [];

    /// <summary>
    /// Создает новый экземпляр комнаты для совместного просмотра
    /// </summary>
    /// <param name="id">Уникальный идентификатор комнаты</param>
    /// <param name="filmId">Идентификатор фильма или сериала для просмотра</param>
    /// <param name="isSerial">Флаг, указывающий является ли контент сериалом</param>
    /// <param name="owner">Владелец комнаты (создатель)</param>
    public Room(Guid id, Guid filmId, bool isSerial, Viewer owner) : base(id)
    {
        FilmId = filmId;
        IsSerial = isSerial;
        Owner = owner;
        Join(owner);
    }

    /// <summary>
    /// Идентификатор фильма или сериала, который просматривается в комнате
    /// </summary>
    public Guid FilmId { get; }

    /// <summary>
    /// Флаг, указывающий является ли контент сериалом
    /// </summary>
    public bool IsSerial { get; }

    /// <summary>
    /// Владелец комнаты (создатель)
    /// </summary>
    public Viewer Owner { get; } = null!;

    /// <summary>
    /// Список всех зрителей в комнате (включая владельца)
    /// </summary>
    public IReadOnlyDictionary<Guid, Viewer> Viewers => _viewersList;

    /// <summary>
    /// Добавляет тег к зрителю для категоризации или поиска
    /// </summary>
    /// <param name="viewerId">Идентификатор зрителя</param>
    /// <param name="tag">Текстовый тег для добавления</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void AddTag(Guid viewerId, string tag)
    {
        var viewer = GetViewer(viewerId);
        viewer.AddTag(tag);
    }

    /// <summary>
    /// Удаляет тег у зрителя
    /// </summary>
    /// <param name="viewerId">Идентификатор зрителя</param>
    /// <param name="tag">Текстовый тег для удаления</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void RemoveTag(Guid viewerId, string tag)
    {
        var viewer = GetViewer(viewerId);
        viewer.RemoveTag(tag);
    }

    /// <summary>
    /// Увеличивает значение статистического параметра зрителя на 1
    /// </summary>
    /// <param name="viewerId">Идентификатор зрителя</param>
    /// <param name="key">Ключ статистического параметра</param>
    /// <returns>Новое значение параметра после инкремента</returns>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public int IncrementStatisticParameter(Guid viewerId, string key)
    {
        var viewer = GetViewer(viewerId);
        var value = viewer.Statistic.GetValueOrDefault(key, 0);
        value++;
        viewer.SetStatistic(key, value);
        return value;
    }

    /// <summary>
    /// Устанавливает фото пользователя.
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="photoKey">Ключ фото в хранилище или null для удаления фото</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetPhoto(Guid targetId, string? photoKey)
    {
        var viewer = GetViewer(targetId);
        SetPhotoInternal(viewer, photoKey);
    }

    /// <summary>
    /// Устанавливает имя пользователя.
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="userName">Новое имя пользователя</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetUserName(Guid targetId, string userName)
    {
        var viewer = GetViewer(targetId);
        SetUserNameInternal(viewer, userName);
    }

    /// <summary>
    /// Устанавливает скорость воспроизведения для зрителя
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="speed">Новая скорость воспроизведения (например, 1.0 - нормальная скорость)</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetSpeed(Guid targetId, double speed)
    {
        var viewer = GetViewer(targetId);
        SetSpeedInternal(viewer, speed, false);
    }

    /// <summary>
    /// Устанавливает статус отключения звука для зрителя
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="muted">Флаг отключения звука (true - звук отключен)</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetMuted(Guid targetId, bool muted)
    {
        var viewer = GetViewer(targetId);
        SetMutedInternal(viewer, muted);
    }

    /// <summary>
    /// Устанавливает настройки пользователя.
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="settings">Новые настройки комнаты</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetSettings(Guid targetId, RoomSettings settings)
    {
        var viewer = GetViewer(targetId);
        SetSettingsInternal(viewer, settings);
    }

    /// <summary>
    /// Устанавливает онлайн-статус зрителя
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="isOnline">Новый статус онлайн (true - онлайн, false - офлайн)</param>
    /// <remarks>
    /// При установке статуса offline автоматически ставит на паузу и выключает полноэкранный режим
    /// для обеспечения корректного состояния при повторном подключении
    /// </remarks>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetOnline(Guid targetId, bool isOnline)
    {
        var viewer = GetViewer(targetId);
        SetOnlineInternal(viewer, isOnline);

        if (isOnline) return;
        SetPauseInternal(viewer, true, true);
        SetFullScreenInternal(viewer, false, true);
    }

    /// <summary>
    /// Устанавливает состояние паузы для зрителя
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="pause">Флаг паузы (true - воспроизведение приостановлено)</param>
    /// <param name="time">Текущее время воспроизведения</param>
    /// <param name="buffering">Флаг, что пауза вызвана дозагрузкой контента</param>
    /// <remarks>
    /// Если зритель является владельцем, генерирует дополнительное событие OwnerPauseChangedEvent
    /// для синхронизации всех участников комнаты
    /// </remarks>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetPause(Guid targetId, bool pause, TimeSpan time, bool buffering)
    {
        var viewer = GetViewer(targetId);
        SetPauseInternal(viewer, pause, false, buffering);
        SetTimeLineInternal(viewer, time, true);
    }

    /// <summary>
    /// Устанавливает полноэкранный режим для зрителя
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="isFullScreen">Флаг полноэкранного режима (true - включен)</param>
    /// <remarks>
    /// Если зритель является владельцем, генерирует дополнительное событие OwnerFullScreenChangedEvent
    /// для оповещения других участников
    /// </remarks>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetFullScreen(Guid targetId, bool isFullScreen)
    {
        var viewer = GetViewer(targetId);
        SetFullScreenInternal(viewer, isFullScreen, false);
    }

    /// <summary>
    /// Устанавливает текущую позицию воспроизведения для зрителя
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="time">Временная позиция в контенте</param>
    /// <remarks>
    /// Если зритель является владельцем, генерирует дополнительное событие OwnerTimeLineChangedEvent
    /// для синхронизации времени воспроизведения у всех участников
    /// </remarks>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void SetTimeLine(Guid targetId, TimeSpan time)
    {
        var viewer = GetViewer(targetId);
        SetTimeLineInternal(viewer, time, false);
    }

    /// <summary>
    /// Устанавливает текущую серию и сезон для зрителя (только для сериалов)
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <param name="season">Номер сезона (должен быть ≥ 1)</param>
    /// <param name="episode">Номер серии (должен быть ≥ 1)</param>
    /// <exception cref="ArgumentOutOfRangeException">Если сезон или серия меньше 1</exception>
    /// <exception cref="ChangeFilmSeriesException">Если контент не является сериалом</exception>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    /// <remarks>
    /// Автоматически сбрасывает таймлайн в 0 и ставит на паузу для корректного начала воспроизведения
    /// новой серии
    /// </remarks>
    public void SetEpisode(Guid targetId, int season, int episode)
    {
        if (season < 1) throw new ArgumentOutOfRangeException(nameof(season), "Season must be positive");
        if (episode < 1) throw new ArgumentOutOfRangeException(nameof(episode), "Episode must be positive");
        if (!IsSerial) throw new ChangeFilmSeriesException();

        var viewer = GetViewer(targetId);

        SetEpisodeInternal(viewer, season, episode, false);
        SetTimeLineInternal(viewer, TimeSpan.Zero, true);
        SetPauseInternal(viewer, true, true);
    }

    /// <summary>
    /// Отправляет звуковой сигнал другому зрителю
    /// </summary>
    /// <param name="initiatorId">Идентификатор инициатора сигнала</param>
    /// <param name="targetId">Идентификатор зрителя, которому отправляется сигнал</param>
    /// <exception cref="ActionNotAllowedException">
    /// Если инициатор пытается сигналить себе или у цели отключены сигналы
    /// </exception>
    /// <exception cref="ViewerNotFoundException">Если инициатор или цель не найдены</exception>
    public void Beep(Guid initiatorId, Guid targetId)
    {
        var initiator = GetViewer(initiatorId);
        var target = GetViewer(targetId);

        if (initiatorId == targetId || !target.Settings.Beep || !initiator.Settings.Beep || !target.Online)
            throw new ActionNotAllowedException(nameof(Beep));

        AddDomainEvent(new ViewerBeepedEvent
        {
            Room = this,
            Initiator = initiator,
            Target = target
        });
    }

    /// <summary>
    /// Отправляет "крик" другому зрителю
    /// </summary>
    /// <param name="initiatorId">Идентификатор инициатора крика</param>
    /// <param name="targetId">Идентификатор зрителя, которому отправляется крик</param>
    /// <exception cref="ActionNotAllowedException">
    /// Если инициатор пытается крикнуть себе или у цели отключены крики
    /// </exception>
    /// <exception cref="ViewerNotFoundException">Если инициатор или цель не найдены</exception>
    public void Scream(Guid initiatorId, Guid targetId)
    {
        var initiator = GetViewer(initiatorId);
        var target = GetViewer(targetId);

        if (initiatorId == targetId || !target.Settings.Screamer || !initiator.Settings.Screamer || !target.Online)
            throw new ActionNotAllowedException(nameof(Scream));

        AddDomainEvent(new ViewerScreamedEvent
        {
            Room = this,
            Initiator = initiator,
            Target = target
        });
    }

    /// <summary>
    /// Исключает зрителя из комнаты
    /// </summary>
    /// <param name="targetId">Идентификатор исключаемого зрителя</param>
    /// <exception cref="ActionNotAllowedException">
    /// Если инициатор пытается исключить себя или не является владельцем
    /// </exception>
    /// <exception cref="ViewerNotFoundException">Если инициатор или цель не найдены</exception>
    public void Kick(Guid targetId)
    {
        var viewer = GetViewer(targetId);
        _viewersList.Remove(targetId);

        AddDomainEvent(new ViewerKickedEvent
        {
            Room = this,
            Target = viewer
        });
    }

    /// <summary>
    /// Подключает нового зрителя к комнате
    /// </summary>
    /// <param name="viewer">Подключаемый зритель</param>
    /// <exception cref="ViewerAlreadyExistsException">Если зритель уже подключен</exception>
    public void Join(Viewer viewer)
    {
        if (!_viewersList.TryAdd(viewer.Id, viewer))
            throw new ViewerAlreadyExistsException();

        AddDomainEvent(new ViewerJoinedEvent
        {
            Room = this,
            Viewer = viewer
        });
    }

    /// <summary>
    /// Отключает зрителя от комнаты
    /// </summary>
    /// <param name="targetId">Идентификатор отключаемого зрителя</param>
    /// <exception cref="ViewerNotFoundException">Если зритель с указанным ID не найден</exception>
    public void Leave(Guid targetId)
    {
        var viewer = GetViewer(targetId);
        _viewersList.Remove(targetId);

        AddDomainEvent(new ViewerLeavedEvent
        {
            Room = this,
            Viewer = viewer
        });
    }

    // Внутренние методы для установки свойств с обновлением тегов

    /// <summary>
    /// Внутренний метод для установки онлайн-статуса с генерацией события при изменении
    /// </summary>
    private void SetOnlineInternal(Viewer viewer, bool isOnline)
    {
        viewer.Online = isOnline;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.Online))) return;
        AddDomainEvent(new ViewerOnlineChangedEvent
        {
            Room = this,
            Viewer = viewer
        });
    }

    /// <summary>
    /// Внутренний метод для установки состояния паузы с генерацией события при изменении
    /// </summary>
    private void SetPauseInternal(Viewer viewer, bool pause, bool isSync, bool buffering = false)
    {
        viewer.OnPause = pause;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.OnPause))) return;
        AddDomainEvent(new ViewerPauseChangedEvent
        {
            Room = this,
            Viewer = viewer,
            IsSyncEvent = isSync,
            Buffering = buffering
        });
    }

    /// <summary>
    /// Внутренний метод для установки полноэкранного режима с генерацией события при изменении
    /// </summary>
    private void SetFullScreenInternal(Viewer viewer, bool isFullScreen, bool isSync)
    {
        viewer.FullScreen = isFullScreen;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.FullScreen))) return;
        AddDomainEvent(new ViewerFullScreenChangedEvent
        {
            Room = this,
            Viewer = viewer,
            IsSyncEvent = isSync
        });
    }

    /// <summary>
    /// Внутренний метод для установки временной позиции с генерацией события при изменении
    /// </summary>
    private void SetTimeLineInternal(Viewer viewer, TimeSpan time, bool isSync)
    {
        viewer.TimeLine = time;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.TimeLine))) return;
        AddDomainEvent(new ViewerTimeLineChangedEvent
        {
            Room = this,
            Viewer = viewer,
            IsSyncEvent = isSync
        });
    }

    /// <summary>
    /// Внутренний метод для установки сезона и серии с генерацией события при изменении
    /// </summary>
    private void SetEpisodeInternal(Viewer viewer, int season, int episode, bool isSync)
    {
        viewer.Season = season;
        viewer.Episode = episode;
        viewer.TimeLine = TimeSpan.Zero;

        var seasonChanged = viewer.ChangedProperties.Contains(nameof(viewer.Season));
        var episodeChanged = viewer.ChangedProperties.Contains(nameof(viewer.Episode));
        var timeChanged = viewer.ChangedProperties.Contains(nameof(viewer.TimeLine));

        if (!seasonChanged && !episodeChanged && !timeChanged) return;
        AddDomainEvent(new ViewerEpisodeChangedEvent
        {
            Room = this,
            Viewer = viewer,
            IsSyncEvent = isSync
        });
    }

    /// <summary>
    /// Внутренний метод для установки скорости воспроизведения с генерацией события при изменении
    /// </summary>
    private void SetSpeedInternal(Viewer viewer, double speed, bool isSync)
    {
        viewer.Speed = speed;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.Speed))) return;

        AddDomainEvent(new ViewerSpeedChangedEvent
        {
            Room = this,
            Viewer = viewer,
            IsSyncEvent = isSync
        });
    }

    /// <summary>
    /// Внутренний метод для установки статуса отключения звука с генерацией события при изменении
    /// </summary>
    private void SetMutedInternal(Viewer viewer, bool muted)
    {
        viewer.Muted = muted;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.Muted))) return;

        AddDomainEvent(new ViewerMuteChangedEvent
        {
            Room = this,
            Viewer = viewer
        });
    }

    /// <summary>
    /// Внутренний метод для установки фото пользователя с генерацией события при изменении
    /// </summary>
    private void SetPhotoInternal(Viewer viewer, string? photoKey)
    {
        viewer.PhotoKey = photoKey;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.PhotoKey))) return;

        AddDomainEvent(new ViewerPhotoChangedEvent
        {
            Room = this,
            Viewer = viewer
        });
    }

    /// <summary>
    /// Внутренний метод для установки имени пользователя с генерацией события при изменении
    /// </summary>
    private void SetUserNameInternal(Viewer viewer, string userName)
    {
        viewer.UserName = userName;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.UserName))) return;

        AddDomainEvent(new ViewerNameChangedEvent
        {
            Room = this,
            Viewer = viewer
        });
    }

    /// <summary>
    /// Внутренний метод для установки настроек пользователя с генерацией события при изменении
    /// </summary>
    private void SetSettingsInternal(Viewer viewer, RoomSettings settings)
    {
        viewer.Settings = settings;
        if (!viewer.ChangedProperties.Contains(nameof(viewer.Settings))) return;

        AddDomainEvent(new ViewerSettingsChangedEvent
        {
            Room = this,
            Viewer = viewer
        });
    }

    /// <summary>
    /// Получает зрителя по идентификатору
    /// </summary>
    /// <param name="targetId">Идентификатор зрителя</param>
    /// <returns>Найденный зритель</returns>
    /// <exception cref="ViewerNotFoundException">Если зритель не найден</exception>
    private Viewer GetViewer(Guid targetId)
    {
        if (!_viewersList.TryGetValue(targetId, out var viewer))
            throw new ViewerNotFoundException();

        return viewer;
    }
}