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

    private string _userName = string.Empty;
    private bool _online;
    private bool _fullScreen;
    private bool _onPause = true;
    private TimeSpan _timeLine;
    private RoomSettings _settings = null!;
    private string? _photoKey;
    private int? _season;
    private int? _episode;
    private double _speed = 1;
    private bool _muted;

    /// <summary>
    /// Уникальный идентификатор зрителя.
    /// </summary>
    public Guid Id { get; } = id;

    /// <summary>
    /// Имя пользователя. Максимальная длина — 40 символов.
    /// </summary>
    public string UserName
    {
        get => _userName;
        internal set
        {
            if (_userName == value) return;
            _userName = value;
            _changedProperties.Add(nameof(UserName));
        }
    }

    /// <summary>
    /// Указывает, находится ли пользователь онлайн.
    /// </summary>
    public bool Online
    {
        get => _online;
        internal set
        {
            if (_online == value) return;
            _online = value;
            _changedProperties.Add(nameof(Online));
        }
    }

    /// <summary>
    /// Указывает, активен ли полноэкранный режим у пользователя.
    /// </summary>
    public bool FullScreen
    {
        get => _fullScreen;
        internal set
        {
            if (_fullScreen == value) return;
            _fullScreen = value;
            _changedProperties.Add(nameof(FullScreen));
        }
    }

    /// <summary>
    /// Указывает, стоит ли видео у пользователя на паузе.
    /// По умолчанию — на паузе.
    /// </summary>
    public bool OnPause
    {
        get => _onPause;
        internal set
        {
            if (_onPause == value) return;
            _onPause = value;
            _changedProperties.Add(nameof(OnPause));
        }
    }

    /// <summary>
    /// Текущая позиция воспроизведения.
    /// </summary>
    public TimeSpan TimeLine
    {
        get => _timeLine;
        internal set
        {
            if (_timeLine == value) return;
            _timeLine = value;
            _changedProperties.Add(nameof(TimeLine));
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public double Speed
    {
        get => _speed;
        internal set
        {
            if (Math.Abs(_speed - value) < 0.1) return;
            _speed = value;
            _changedProperties.Add(nameof(Speed));
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public bool Muted
    {
        get => _muted;
        set
        {
            if (_muted == value) return;
            _muted = value;
            _changedProperties.Add(nameof(Muted));
        }
    }

    /// <summary>
    /// Набор разрешений.
    /// </summary>
    public RoomSettings Settings
    {
        get => _settings;
        internal set
        {
            if (_settings == value) return;
            _settings = value;
            _changedProperties.Add(nameof(Settings));
        }
    }

    /// <summary>
    /// Ключ фотографии пользователя.
    /// </summary>
    public string? PhotoKey
    {
        get => _photoKey;
        internal set
        {
            if (_photoKey == value) return;
            _photoKey = value;
            _changedProperties.Add(nameof(PhotoKey));
        }
    }

    /// <summary>
    /// Текущий сезон.
    /// </summary>
    public int? Season
    {
        get => _season;
        internal set
        {
            if (_season == value) return;
            _season = value;
            _changedProperties.Add(nameof(Season));
        }
    }

    /// <summary>
    /// Текущая серия.
    /// </summary>
    public int? Episode
    {
        get => _episode;
        internal set
        {
            if (_episode == value) return;
            _episode = value;
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