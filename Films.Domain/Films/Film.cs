using Common.Domain.Aggregates;
using Common.Domain.Extensions;
using Films.Domain.Films.Exceptions;
using Films.Domain.Films.ValueObjects;

namespace Films.Domain.Films;

/// <summary>
/// Класс, представляющий фильм.
/// </summary>
public partial class Film(Guid id) : AggregateRoot(id)
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 1500;
    private const int MaxShortDescriptionLength = 500;
    private const int MaxCountriesLength = 100;
    private const int MaxGenresLength = 100;
    private const int MaxPersonLength = 100;

    #region Info

    /// <summary> 
    /// Заголовок фильма. 
    /// </summary> 
    private readonly string _title = null!;

    /// <summary> 
    /// Описание фильма. 
    /// </summary> 
    private string _description = null!;

    /// <summary> 
    /// Краткое описание фильма. 
    /// </summary> 
    private string? _shortDescription;

    /// <summary> 
    /// Заголовок фильма. 
    /// </summary> 
    public required string Title
    {
        get => _title;
        init => _title = value.ValidateLength(nameof(Title), MaxTitleLength);
    }

    /// <summary> 
    /// Описание фильма. 
    /// </summary> 
    public required string Description
    {
        get => _description;
        set => _description = value.ValidateLength(nameof(Description), MaxDescriptionLength);
    }

    /// <summary> 
    /// Краткое описание фильма. 
    /// </summary> 
    public string ShortDescription
    {
        get
        {
            if (!string.IsNullOrEmpty(_shortDescription)) return _shortDescription;
            if (_description.Length < 100) return _description;
            return _description[..100] + "...";
        }
        set => _shortDescription = value.ValidateLength(nameof(ShortDescription), MaxShortDescriptionLength);
    }

    /// <summary> 
    /// Год выпуска фильма. 
    /// </summary> 
    public required DateOnly Date { get; init; }

    /// <summary> 
    /// URL постера фильма. 
    /// </summary> 
    public required string PosterKey { get; set; }

    #endregion

    #region Collections

    /// <summary> 
    /// Список стран, связанных с фильмом. 
    /// </summary> 
    private readonly HashSet<string> _countries = null!;

    /// <summary> 
    /// Список режиссеров фильма. 
    /// </summary> 
    private readonly HashSet<string> _directors = null!;

    /// <summary> 
    /// Список сценаристов фильма. 
    /// </summary> 
    private readonly HashSet<string> _screenwriters = null!;

    /// <summary> 
    /// Список актеров фильма. 
    /// </summary> 
    private readonly HashSet<Actor> _actors = null!;

    /// <summary> 
    /// Список жанров фильма. 
    /// </summary> 
    private readonly HashSet<string> _genres = null!;

    /// <summary> 
    /// Жанры фильма. 
    /// </summary> 
    public required IReadOnlyCollection<string> Genres
    {
        get => _genres;
        init
        {
            if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Genres));
            var set = new HashSet<string>(value.Count);
            foreach (var genre in value)
            {
                genre.ValidateLength(nameof(Genres), MaxGenresLength);
                set.Add(genre);
            }
            _genres = set;
        }
    }

    /// <summary> 
    /// Страны фильма. 
    /// </summary> 
    public required IReadOnlyCollection<string> Countries
    {
        get => _countries;
        init
        {
            if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Countries));
            var set = new HashSet<string>(value.Count);
            foreach (var country in value)
            {
                country.ValidateLength(nameof(Countries), MaxCountriesLength);
                set.Add(country);
            }
            _countries = set;
        }
    }

    /// <summary> 
    /// Режиссеры фильма. 
    /// </summary> 
    public required IReadOnlyCollection<string> Directors
    {
        get => _directors;
        init
        {
            if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Directors));
            var set = new HashSet<string>(value.Count);
            foreach (var director in value)
            {
                director.ValidateLength(nameof(Directors), MaxPersonLength);
                set.Add(director);
            }
            _directors = set;
        }
    }

    /// <summary> 
    /// Актеры фильма. 
    /// </summary> 
    public required IReadOnlyCollection<Actor> Actors
    {
        get => _actors;
        init
        {
            if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Actors));
            _actors = value.ToHashSet();
        }
    }

    /// <summary> 
    /// Сценаристы фильма. 
    /// </summary> 
    public required IReadOnlyCollection<string> Screenwriters
    {
        get => _screenwriters;
        init
        {
            if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Screenwriters));
            var set = new HashSet<string>(value.Count);
            foreach (var screenwriter in value)
            {
                screenwriter.ValidateLength(nameof(Screenwriters), MaxPersonLength);
                set.Add(screenwriter);
            }
            _screenwriters = set;
        }
    }

    #endregion

    #region Rating

    /// <summary> 
    /// Рейтинг фильма на КиноПоиске. 
    /// </summary> 
    public Rating? RatingKp { get; set; }

    /// <summary> 
    /// Рейтинг фильма на IMDb. 
    /// </summary> 
    public Rating? RatingImdb { get; set; }

    #endregion

    #region Type

    /// <summary>
    /// Описание фильма или другого неделимого медиаконтента (например, шоу, передача).
    /// Задаётся только если это не сериал.
    /// Взаимоисключающее свойство с <see cref="Seasons"/>.
    /// </summary>
    public MediaContent? Content { get; private set; }

    /// <summary>
    /// Список сезонов сериала.
    /// Задаётся только если это сериал.
    /// Взаимоисключающее свойство с <see cref="Content"/>.
    /// </summary>
    public IReadOnlySet<Season>? Seasons { get; private set; }

    /// <summary>
    /// Флаг, является ли фильм сериалом
    /// </summary>
    public bool IsSerial => Content == null && Seasons is { Count: > 0 };

    /// <summary>
    /// Флаг, может ли быть создана комната с этим фильмом.
    /// </summary>
    public bool CanCreateRoom => Content != null || Seasons is { Count: > 0 };

    /// <summary>
    /// Добавляет новую версию медиаконтента (для фильма или конкретного эпизода сериала)
    /// </summary>
    /// <param name="version">Название версии (например, "Оригинал", "Режиссерская версия")</param>
    /// <param name="seasonNumber">Номер сезона (только для сериалов)</param>
    /// <param name="episodeNumber">Номер эпизода (только для сериалов)</param>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается при попытке добавить версию к фильму, когда установлены сезоны, и наоборот
    /// </exception>
    public void AddVersion(string version, int? seasonNumber = null, int? episodeNumber = null)
    {
        // Проверяем, добавляем ли мы версию для эпизода сериала
        if (seasonNumber.HasValue && episodeNumber.HasValue)
        {
            // Если это не сериал (уже есть контент), выбрасываем исключение
            if (Content != null)
                throw new InvalidOperationException(
                    "Нельзя добавить версию эпизода к фильму. Используйте метод без указания сезона/эпизода.");

            // Создаем отсортированный набор для версий с нашей новой версией
            var updatedVersions = new SortedSet<string> { version };

            // Создаем новый эпизод с этой версией
            var updatedEpisodes = new SortedSet<Episode>
            {
                new()
                {
                    Versions = updatedVersions,
                    Number = episodeNumber.Value
                }
            };

            // Создаем новый сезон с этим эпизодом
            var updatedSeasons = new SortedSet<Season>
            {
                new()
                {
                    Number = seasonNumber.Value,
                    Episodes = updatedEpisodes
                }
            };

            // Если сезонов еще нет, просто устанавливаем новый сезон
            if (Seasons == null)
            {
                Seasons = updatedSeasons;
                return;
            }

            // Если сезоны уже существуют, выполняем слияние структур
            foreach (var season in Seasons)
            {
                // Пытаемся добавить текущий сезон из существующих в обновленную коллекцию
                // Add вернет false, если сезон с таким номером уже есть (значит это наш обновляемый сезон)
                var isExistingSeason = !updatedSeasons.Add(season);

                // Если это не наш сезон (добавился успешно), переходим к следующему
                if (!isExistingSeason) continue;

                // Обрабатываем эпизоды сезона, который мы обновляем
                foreach (var episode in season.Episodes)
                {
                    // Пытаемся добавить текущий эпизод в обновленную коллекцию
                    // Add вернет false, если эпизод с таким номером уже есть (наш обновляемый эпизод)
                    var isExistingEpisode = !updatedEpisodes.Add(episode);

                    // Если это не наш эпизод (добавился успешно), переходим к следующему
                    if (!isExistingEpisode) continue;

                    // Для нашего эпизода добавляем все его существующие версии
                    // SortedSet автоматически исключит дубликаты по имени версии
                    foreach (var mediaVersion in episode.Versions)
                    {
                        // Добавляем существующие версии
                        updatedVersions.Add(mediaVersion);
                    }
                }
            }

            // Обновляем коллекцию сезонов
            Seasons = updatedSeasons;
        }
        else
        {
            // Это добавление версии для фильма (не сериала)
            // Если уже есть сезоны, выбрасываем исключение
            if (Seasons != null)
                throw new InvalidOperationException(
                    "Нельзя добавить версию фильма к сериалу. Укажите номер сезона и эпизода.");

            // Создаем набор версий с нашей новой версией
            var updatedVersions = new SortedSet<string> { version };

            // Создаем новый медиаконтент с этой версией
            var updatedContent = new MediaContent
            {
                Versions = updatedVersions
            };

            // Если контент уже существует, добавляем все его существующие версии
            if (Content != null)
            {
                foreach (var mediaVersion in Content.Versions)
                {
                    updatedVersions.Add(mediaVersion);
                }
            }

            // Устанавливаем обновленный контент
            Content = updatedContent;
        }
    }

    #endregion
}