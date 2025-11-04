using Common.Domain.Aggregates;
using Common.Domain.Extensions;
using Common.Domain.Rooms;
using Films.Domain.Films;
using Films.Domain.Users.Events;
using Films.Domain.Users.ValueObjects;

namespace Films.Domain.Users;

/// <summary>
/// Класс, представляющий пользователя системы
/// </summary>
/// <param name="id">Уникальный идентификатор пользователя</param>
public partial class User(Guid id) : AggregateRoot(id)
{
    /// <summary>
    /// Максимальная длина имени пользователя
    /// </summary>
    private const int MaxUsernameLength = 200;

    /// <summary>
    /// Поле для хранения имени пользователя
    /// </summary>
    private string _username = null!;

    /// <summary>
    /// Настройки разрешений пользователя
    /// </summary>
    private RoomSettings _settings = new()
    {
        Beep = true,
        Screamer = true
    };

    /// <summary>
    /// Имя пользователя
    /// </summary>
    /// <remarks>
    /// При установке значения выполняется проверка на максимальную длину
    /// </remarks>
    public required string Username
    {
        get => _username;
        set => _username = value.ValidateLength(nameof(Username), MaxUsernameLength);
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
        get => _settings;
        set
        {
            if (_settings == value) return;
            _settings = value;
            AddDomainEvent(new UserSettingsChangedEvent(this));
        }
    }

    /// <summary>
    /// Коллекция фильмов в списке желаемого
    /// </summary>
    private readonly HashSet<FilmNote> _watchlist = [];

    /// <summary>
    /// Коллекция просмотренных фильмов
    /// </summary>
    private readonly HashSet<FilmNote> _history = [];

    /// <summary>
    /// Коллекция предпочитаемых жанров
    /// </summary>
    private HashSet<string> _genres = [];

    /// <summary>
    /// Список желаемого (отсортированный по дате добавления)
    /// </summary>
    public IReadOnlyCollection<FilmNote> Watchlist => _watchlist.OrderByDescending(x => x.Date).ToArray();

    /// <summary>
    /// История просмотров (отсортированная по дате просмотра)
    /// </summary>
    public IReadOnlyCollection<FilmNote> History => _history.OrderByDescending(x => x.Date).ToArray();

    /// <summary>
    /// Предпочитаемые жанры пользователя
    /// </summary>
    public IReadOnlyCollection<string> Genres => _genres;

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
        // Удаляем фильм, если он уже есть в списке
        _watchlist.RemoveWhere(x => x.FilmId == film.Id);

        // Если список переполнен - удаляем самый старый элемент
        if (_watchlist.Count > 14)
            _watchlist.Remove(_watchlist.OrderBy(x => x.Date).First());

        // Добавляем новый фильм
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
        // Удаляем фильм, если он уже есть в истории
        _history.RemoveWhere(x => x.FilmId == film.Id);

        // Если история переполнена - удаляем самый старый просмотр
        if (_history.Count > 5)
            _history.Remove(_history.OrderBy(x => x.Date).First());

        // Добавляем новый просмотр
        _history.Add(new FilmNote
        {
            FilmId = film.Id
        });
    }

    /// <summary>
    /// Обновляет предпочитаемые жанры на основе оцененных фильмов
    /// </summary>
    /// <param name="films">Список фильмов для анализа жанров</param>
    /// <remarks>
    /// Выбирает 5 самых часто встречающихся жанров из переданных фильмов
    /// </remarks>
    public void UpdateGenres(IReadOnlyList<FilmToUpdate> films)
    {
        _genres = films
            // Получаем все жанры из всех фильмов
            .SelectMany(x => x.Genres)

            // Группируем жанры по названию
            .GroupBy(g => g)

            // Сортируем по частоте встречаемости (убывание)
            .OrderByDescending(genre => genre.Count())

            // Берем только названия жанров
            .Select(x => x.Key)

            // Ограничиваем топ-5 жанров
            .Take(5)

            // Преобразуем в HashSet
            .ToHashSet();
    }

    /// <summary>
    /// Вспомогательная запись для обновления жанров
    /// </summary>
    /// <param name="Genres">Массив жанров фильма</param>
    public record FilmToUpdate(string[] Genres);
}