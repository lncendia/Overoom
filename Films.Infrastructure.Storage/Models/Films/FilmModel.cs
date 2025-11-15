using Films.Domain.Films.Snapshots;
using Films.Domain.Films.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель фильма для работы с базой данных.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
[BsonIgnoreExtraElements]
public class FilmModel : VersionedUpdatedEntity<FilmModel>
{
    private MediaContentModel? _content;
    private TrackedValueObjectCollection<SeasonModel, FilmModel>? _seasons;
    private TrackedCollection<string, FilmModel> _genres = new();
    private TrackedCollection<string, FilmModel> _countries = new();
    private TrackedCollection<Actor, FilmModel> _actors = new();
    private TrackedCollection<string, FilmModel> _directors = new();
    private TrackedCollection<string, FilmModel> _screenwriters = new();

    /// <summary>
    /// Уникальный идентификатор фильма
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Название фильма (максимальная длина - 200 символов)
    /// </summary>
    public string Title
    {
        get;
        set => field = TrackChange(nameof(Title), field, value)!;
    } = null!;

    /// <summary>
    /// Ссылка на постер фильма
    /// </summary>
    public string PosterKey
    {
        get;
        set => field = TrackChange(nameof(PosterKey), field, value)!;
    } = null!;

    /// <summary>
    /// Полное описание фильма (максимальная длина - 1500 символов)
    /// </summary>
    public string Description
    {
        get;
        set => field = TrackChange(nameof(Description), field, value)!;
    } = null!;

    /// <summary>
    /// Краткое описание фильма (максимальная длина - 500 символов)
    /// </summary>
    public string ShortDescription
    {
        get;
        set => field = TrackChange(nameof(ShortDescription), field, value)!;
    } = null!;

    /// <summary>
    /// Год выпуска фильма
    /// </summary>
    public DateTime Date
    {
        get;
        set => field = TrackStructChange(nameof(Date), field, value);
    }

    /// <summary>
    /// Рейтинг фильма на КиноПоиске
    /// </summary>
    public double? RatingKp
    {
        get;
        set => field = TrackStructChange(nameof(RatingKp), field, value);
    }

    /// <summary>
    /// Рейтинг фильма на IMDb
    /// </summary>
    public double? RatingImdb
    {
        get;
        set => field = TrackStructChange(nameof(RatingImdb), field, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public MediaContentModel? Content
    {
        get => _content;
        set => _content = TrackValueObject(nameof(Content), _content, value);
    }

    /// <summary>
    /// Список CDN-источников для фильма
    /// </summary>
    public List<SeasonModel>? Seasons
    {
        get => _seasons?.Collection;
        set => _seasons = TrackValueObjectCollection(nameof(Seasons), _seasons, value)!;
    }

    /// <summary>
    /// Список жанров фильма
    /// </summary>
    public List<string> Genres
    {
        get => _genres.Collection;
        set => _genres = TrackCollection(nameof(Genres), _genres, value)!;
    }

    /// <summary>
    /// Список стран производства фильма
    /// </summary>
    public List<string> Countries
    {
        get => _countries.Collection;
        set => _countries = TrackCollection(nameof(Countries), _countries, value)!;
    }

    /// <summary>
    /// Список актеров фильма
    /// </summary>
    public List<Actor> Actors
    {
        get => _actors.Collection;
        set => _actors = TrackCollection(nameof(Actors), _actors, value)!;
    }

    /// <summary>
    /// Список режиссеров фильма
    /// </summary>
    public List<string> Directors
    {
        get => _directors.Collection;
        set => _directors = TrackCollection(nameof(Directors), _directors, value)!;
    }

    /// <summary>
    /// Список сценаристов фильма
    /// </summary>
    public List<string> Screenwriters
    {
        get => _screenwriters.Collection;
        set => _screenwriters = TrackCollection(nameof(Screenwriters), _screenwriters, value)!;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает определение для обновления MongoDB, объединяя изменения всех отслеживаемых свойств
    /// </summary>
    public override UpdateDefinition<FilmModel> UpdateDefinition
    {
        get
        {
            var baseUpdate = base.UpdateDefinition;

            var contentUpdate = _content?.GetUpdateDefinition(null, nameof(Content), AddedValueObjects);
            var seasonsUpdate = _seasons?.GetUpdateDefinition(null, nameof(Seasons), AddedValueObjects);
            var genresUpdate = _genres.GetUpdateDefinition(null, nameof(Genres), AddedValueObjects);
            var countriesUpdate = _countries.GetUpdateDefinition(null, nameof(Countries), AddedValueObjects);
            var actorsUpdate = _actors.GetUpdateDefinition(null, nameof(Actors), AddedValueObjects);
            var directorsUpdate = _directors.GetUpdateDefinition(null, nameof(Directors), AddedValueObjects);
            var screenwritersUpdate =
                _screenwriters.GetUpdateDefinition(null, nameof(Screenwriters), AddedValueObjects);

            return Combine(
                baseUpdate,
                contentUpdate,
                seasonsUpdate,
                genresUpdate,
                countriesUpdate,
                actorsUpdate,
                directorsUpdate,
                screenwritersUpdate
            );
        }
    }

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает текущее состояние сущности, объединяя состояния всех отслеживаемых свойств
    /// </summary>
    public override EntityState EntityState => Combine(
        _content?.IsModified,
        _seasons?.IsModified,
        _genres.IsModified,
        _countries.IsModified,
        _actors.IsModified,
        _directors.IsModified,
        _screenwriters.IsModified
    );

    /// <inheritdoc/>
    /// <summary>
    /// Очищает все изменения в сущности и всех отслеживаемых свойствах
    /// </summary>
    public override void ClearChanges()
    {
        base.ClearChanges();
        _content?.ClearChanges();
        _seasons?.ClearChanges();
        _genres.ClearChanges();
        _countries.ClearChanges();
        _actors.ClearChanges();
        _directors.ClearChanges();
        _screenwriters.ClearChanges();
    }

    public FilmSnapshot GetSnapshot()
    {
        var content = Content == null
            ? null
            : new MediaContent { Versions = Content.Versions.ToHashSet() };

        var seasons = Seasons?.Select(s => new Season
        {
            Number = s.Number,
            Episodes = s.Episodes.Select(e => new Episode
            {
                Number = e.Number,
                Versions = e.Versions.ToHashSet()
            }).ToHashSet()
        }).ToHashSet();

        return new FilmSnapshot
        {
            Id = Id,
            Title = Title,
            Description = Description,
            ShortDescription = ShortDescription,
            Date = new DateOnly(Date.Year, Date.Month, Date.Day),
            PosterKey = PosterKey,
            RatingKp = RatingKp.HasValue ? new Rating(RatingKp.Value) : null,
            RatingImdb = RatingImdb.HasValue ? new Rating(RatingImdb.Value) : null,
            Content = content,
            Seasons = seasons,
            Genres = Genres,
            Countries = Countries,
            Actors = Actors,
            Directors = Directors,
            Screenwriters = Screenwriters
        };
    }


    public void UpdateFromSnapshot(FilmSnapshot snapshot)
    {
        Title = snapshot.Title;
        Description = snapshot.Description;
        ShortDescription = snapshot.ShortDescription;
        Date = new DateTime(snapshot.Date, TimeOnly.MinValue);
        PosterKey = snapshot.PosterKey;
        RatingKp = snapshot.RatingKp?.Value;
        RatingImdb = snapshot.RatingImdb?.Value;
        Genres = snapshot.Genres.ToList();
        Countries = snapshot.Countries.ToList();
        Actors = snapshot.Actors.ToList();
        Directors = snapshot.Directors.ToList();
        Screenwriters = snapshot.Screenwriters.ToList();

        if (snapshot.Content != null)
        {
            Content ??= new MediaContentModel();
            Content.Versions = snapshot.Content.Versions.ToList();
        }
        else
        {
            Content = null;
        }

        if (snapshot.Seasons != null)
        {
            Seasons = snapshot.Seasons.Select(@as =>
            {
                // Находим существующую модель сезона по номеру или создаем новую
                var season = Seasons?.FirstOrDefault(ms => @as.Number == ms.Number) ??
                             new SeasonModel { Number = @as.Number };

                // Обрабатываем эпизоды сезона
                season.Episodes = @as.Episodes.Select(ae =>
                {
                    // Находим существующую модель эпизода по номеру или создаем новую
                    var episode = season.Episodes.FirstOrDefault(me => ae.Number == me.Number) ??
                                  new EpisodeModel { Number = ae.Number };

                    episode.Versions = ae.Versions.ToList();
                    return episode;
                }).ToList();

                return season;
            }).ToList();
        }
        else
        {
            Seasons = null;
        }
    }
}