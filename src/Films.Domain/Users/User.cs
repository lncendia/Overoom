using Common.Domain.Aggregates;
using Common.Domain.Extensions;
using Common.Domain.Rooms;

using Films.Domain.Films;
using Films.Domain.Users.Events;
using Films.Domain.Users.Snapshots;
using Films.Domain.Users.ValueObjects;

namespace Films.Domain.Users;

/// <summary>
/// Класс, представляющий пользователя системы
/// </summary>
public partial class User : AggregateRoot
{
  #region Константы

  /// <summary>
  /// Максимальная длина имени пользователя
  /// </summary>
  private const int MaxUsernameLength = 200;

  #endregion

  #region Поля и свойства

  /// <summary>
  /// Имя пользователя
  /// </summary>
  /// <remarks>
  /// При установке значения выполняется проверка на максимальную длину
  /// </remarks>
  public required string Username
  {
    get;
    set => field = value.ValidateLength(nameof(Username), MaxUsernameLength);
  }

  /// <summary>
  /// Ключ фотографии пользователя в хранилище
  /// </summary>
  public string? PhotoKey { get; set; }

  /// <summary>
  /// Настройки разрешений пользователя
  /// </summary>
  public RoomSettings RoomSettings
  {
    get;
    set
    {
      if (field == value) return;
      field = value;
      AddDomainEvent(new UserSettingsChangedEvent(this));
    }
  } = new()
  {
    Beep = true,
    Screamer = true
  };

  /// <summary>
  /// Коллекция фильмов в списке желаемого
  /// </summary>
  private readonly HashSet<FilmNote> _watchlist = [];

  /// <summary>
  /// Коллекция просмотренных фильмов
  /// </summary>
  private readonly HashSet<FilmNote> _history = [];

  /// <summary>
  /// Список желаемого (отсортированный по дате добавления)
  /// </summary>
  public IReadOnlyCollection<FilmNote> Watchlist => [.. _watchlist.OrderByDescending(x => x.Date)];

  /// <summary>
  /// История просмотров (отсортированная по дате просмотра)
  /// </summary>
  public IReadOnlyCollection<FilmNote> History => [.. _history.OrderByDescending(x => x.Date)];

  #endregion

  #region Методы

  /// <summary>
  /// Добавляет или удаляет фильм из списка желаемого
  /// </summary>
  /// <param name="film">Фильм для добавления/удаления</param>
  /// <remarks>
  /// Если фильм уже есть в списке - он будет удален.
  /// Список ограничен 15 элементами - при превышении удаляется самый старый.
  /// </remarks>
  public void ToggleWatchlist(Film film)
  {
    if (_watchlist.RemoveWhere(x => x.FilmId == film.Id) > 0) return;

    if (_watchlist.Count > 14)
      _watchlist.Remove(_watchlist.OrderBy(x => x.Date).First());

    _watchlist.Add(new FilmNote
    {
      FilmId = film.Id
    });
  }

  /// <summary>
  /// Добавляет фильм в историю просмотров
  /// </summary>
  /// <param name="film">Просмотренный фильм</param>
  /// <remarks>
  /// История ограничена 6 элементами - при превышении удаляется самый старый просмотр.
  /// </remarks>
  public void AddFilmToHistory(Film film)
  {
    _history.RemoveWhere(x => x.FilmId == film.Id);

    if (_history.Count > 5)
      _history.Remove(_history.OrderBy(x => x.Date).First());

    _history.Add(new FilmNote
    {
      FilmId = film.Id
    });
  }

  #endregion

  #region Конструкторы

  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="id">Уникальный идентификатор пользователя</param>
  public User(Guid id) : base(id)
  {
  }

  #endregion
}
