using Common.Domain.Rooms;

namespace Rooms.Domain.Rooms.Entities;

/// <summary>
/// Представляет зрителя в комнате совместного просмотра.
/// Отслеживает изменения всех полей и коллекций.
/// </summary>
public partial class Viewer(Guid id)
{
    private readonly HashSet<string> _changedProperties = [];

    /// <summary>
    /// Поля, которые были изменены с момента создания объекта или последнего сброса.
    /// </summary>
    public IReadOnlySet<string> ChangedProperties => _changedProperties;

    /// <summary>
    /// Уникальный идентификатор зрителя.
    /// </summary>
    public Guid Id { get; } = id;

    /// <summary>
    /// Имя пользователя. Максимальная длина — 40 символов.
    /// </summary>
    public string UserName
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(UserName));
        }
    } = string.Empty;

    /// <summary>
    /// Указывает, находится ли пользователь онлайн.
    /// </summary>
    public bool Online
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(Online));
        }
    }

    /// <summary>
    /// Указывает, активен ли полноэкранный режим у пользователя.
    /// </summary>
    public bool FullScreen
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(FullScreen));
        }
    }

    /// <summary>
    /// Указывает, стоит ли видео у пользователя на паузе.
    /// По умолчанию — на паузе.
    /// </summary>
    public bool OnPause
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(OnPause));
        }
    } = true;

    /// <summary>
    /// Текущая позиция воспроизведения.
    /// </summary>
    public TimeSpan TimeLine
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(TimeLine));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public double Speed
    {
        get;
        internal set
        {
            if (Math.Abs(field - value) < 0.1) return;
            field = value;
            _changedProperties.Add(nameof(Speed));
        }
    } = 1;

    /// <summary>
    ///
    /// </summary>
    public bool Muted
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(Muted));
        }
    }

    /// <summary>
    /// Набор разрешений.
    /// </summary>
    public RoomSettings Settings
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(Settings));
        }
    } = null!;

    /// <summary>
    /// Ключ фотографии пользователя.
    /// </summary>
    public string? PhotoKey
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(PhotoKey));
        }
    }

    /// <summary>
    /// Текущий сезон.
    /// </summary>
    public int? Season
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(Season));
        }
    }

    /// <summary>
    /// Текущая серия.
    /// </summary>
    public int? Episode
    {
        get;
        internal set
        {
            if (field == value) return;
            field = value;
            _changedProperties.Add(nameof(Episode));
        }
    }

    // ---------- Коллекции с обёртками для отслеживания изменений ----------

    private readonly HashSet<string> _tagsSet = [];
    private readonly Dictionary<string, int> _statisticDictionary = [];

    /// <summary>
    /// Теги зрителя.
    /// </summary>
    public IReadOnlySet<string> Tags => _tagsSet;

    /// <summary>
    /// Статистика зрителя.
    /// </summary>
    public IReadOnlyDictionary<string, int> Statistic => _statisticDictionary;

    /// <summary>
    /// Добавить тег.
    /// </summary>
    internal void AddTag(string tag)
    {
        var added = _tagsSet.Add(tag);
        if (added) _changedProperties.Add(nameof(Tags));
    }

    /// <summary>
    /// Удалить тег.
    /// </summary>
    internal void RemoveTag(string tag)
    {
        var removed = _tagsSet.Remove(tag);
        if (removed) _changedProperties.Add(nameof(Tags));
    }

    /// <summary>
    /// Установить значение статистики.
    /// </summary>
    internal void SetStatistic(string key, int value)
    {
        _statisticDictionary[key] = value;
        _changedProperties.Add(nameof(Statistic));
    }
}