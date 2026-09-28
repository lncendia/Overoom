using System.Diagnostics.CodeAnalysis;

using Common.Domain.Aggregates;
using Common.Domain.Extensions;

using Films.Domain.Films.Exceptions;
using Films.Domain.Films.Snapshots;
using Films.Domain.Films.ValueObjects;

namespace Films.Domain.Films;

/// <summary>
/// Класс, представляющий фильм.
/// </summary>
public partial class Film : AggregateRoot
{
  #region Константы

  private const int MaxTitleLength = 200;
  private const int MaxDescriptionLength = 1500;
  private const int MaxShortDescriptionLength = 500;
  private const int MaxCountriesLength = 100;
  private const int MaxGenresLength = 100;
  private const int MaxPersonLength = 100;

  #endregion

  #region Поля и свойства

  /// <summary>
  /// Описание фильма.
  /// </summary>
  private string _description = null!;

  /// <summary>
  /// Заголовок фильма.
  /// </summary>
  public required string Title
  {
    get;
    init => field = value.ValidateLength(nameof(Title), MaxTitleLength);
  }

  /// <summary>
  /// Описание фильма.
  /// </summary>
  public required string? Description
  {
    get => _description;
    set => _description = value.ValidateLength(nameof(Description), MaxDescriptionLength);
  }

  /// <summary>
  /// Краткое описание фильма.
  /// </summary>
  [field: AllowNull]
  public string ShortDescription
  {
    get
    {
      if (!string.IsNullOrEmpty(field)) return field;
      if (_description.Length < 100) return _description;
      return _description[..100] + "...";
    }
    set => field = value.ValidateLength(nameof(ShortDescription), MaxShortDescriptionLength);
  }

  /// <summary>
  /// Год выпуска фильма.
  /// </summary>
  public required DateOnly Date { get; init; }

  /// <summary>
  /// URL постера фильма.
  /// </summary>
  public required string PosterKey { get; set; }

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
  public required IReadOnlyCollection<string>? Genres
  {
    get => _genres;
    init
    {
      if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Genres));
      var set = new HashSet<string>(value.Count);
      foreach (string genre in value)
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
  public required IReadOnlyCollection<string>? Countries
  {
    get => _countries;
    init
    {
      if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Countries));
      var set = new HashSet<string>(value.Count);
      foreach (string country in value)
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
  public required IReadOnlyCollection<string>? Directors
  {
    get => _directors;
    init
    {
      if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Directors));
      var set = new HashSet<string>(value.Count);
      foreach (string director in value)
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
  public required IReadOnlyCollection<Actor>? Actors
  {
    get => _actors;
    init
    {
      if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Actors));
      _actors = [.. value];
    }
  }

  /// <summary>
  /// Сценаристы фильма.
  /// </summary>
  public required IReadOnlyCollection<string>? Screenwriters
  {
    get => _screenwriters;
    init
    {
      if (value.Count == 0) throw new EmptyTagsCollectionException(nameof(Screenwriters));
      var set = new HashSet<string>(value.Count);
      foreach (string screenwriter in value)
      {
        screenwriter.ValidateLength(nameof(Screenwriters), MaxPersonLength);
        set.Add(screenwriter);
      }

      _screenwriters = set;
    }
  }

  /// <summary>
  /// Рейтинг фильма на КиноПоиске.
  /// </summary>
  public Rating? RatingKp { get; set; }

  /// <summary>
  /// Рейтинг фильма на IMDb.
  /// </summary>
  public Rating? RatingImdb { get; set; }

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

  #endregion

  #region Методы

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
    if (seasonNumber.HasValue && episodeNumber.HasValue)
    {
      if (Content != null)
        throw new InvalidOperationException(
          "You can't add a version of an episode to a movie. Use the method without specifying the season/episode.");

      var updatedVersions = new SortedSet<string> { version };

      var updatedEpisodes = new SortedSet<Episode>
      {
        new()
        {
          Versions = updatedVersions,
          Number = episodeNumber.Value
        }
      };

      var updatedSeasons = new SortedSet<Season>
      {
        new()
        {
          Number = seasonNumber.Value,
          Episodes = updatedEpisodes
        }
      };

      if (Seasons == null)
      {
        Seasons = updatedSeasons;
        return;
      }

      foreach (Season season in Seasons)
      {
        // Add вернет false, если сезон с таким номером уже есть (значит это наш обновляемый сезон)
        bool isExistingSeason = !updatedSeasons.Add(season);
        if (!isExistingSeason) continue;

        foreach (Episode episode in season.Episodes)
        {
          // Add вернет false, если эпизод с таким номером уже есть (наш обновляемый эпизод)
          bool isExistingEpisode = !updatedEpisodes.Add(episode);
          if (!isExistingEpisode) continue;

          // SortedSet автоматически исключит дубликаты по имени версии
          foreach (string mediaVersion in episode.Versions)
          {
            updatedVersions.Add(mediaVersion);
          }
        }
      }

      Seasons = updatedSeasons;
    }
    else
    {
      if (Seasons != null)
        throw new InvalidOperationException(
          "You can't add a movie version to a TV series. Specify the season and episode number.");

      var updatedVersions = new SortedSet<string> { version };

      var updatedContent = new MediaContent
      {
        Versions = updatedVersions
      };

      if (Content != null)
      {
        foreach (string mediaVersion in Content.Versions)
        {
          updatedVersions.Add(mediaVersion);
        }
      }

      Content = updatedContent;
    }
  }

  #endregion

  #region Конструкторы

  /// <summary>
  /// Конструктор
  /// </summary>
  public Film(Guid id) : base(id)
  {
  }

  #endregion
}
