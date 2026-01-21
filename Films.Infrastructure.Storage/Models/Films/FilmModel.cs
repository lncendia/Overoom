using Films.Domain.Films.Snapshots;
using Films.Domain.Films.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель фильма для работы с базой данных.
/// </summary>
[BsonIgnoreExtraElements]
public class FilmModel
{
  /// <summary>
  /// Уникальный идентификатор фильма
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Название фильма
  /// </summary>
  public required string Title { get; set; }

  /// <summary>
  /// Ссылка на постер фильма
  /// </summary>
  public required string PosterKey { get; set; }

  /// <summary>
  /// Полное описание фильма
  /// </summary>
  public required string Description { get; set; }

  /// <summary>
  /// Краткое описание фильма
  /// </summary>
  public required string ShortDescription { get; set; }

  /// <summary>
  /// Год выпуска фильма
  /// </summary>
  public DateTime Date { get; set; }

  /// <summary>
  /// Рейтинг фильма на КиноПоиске
  /// </summary>
  public double? RatingKp { get; set; }

  /// <summary>
  /// Рейтинг фильма на IMDb
  /// </summary>
  public double? RatingImdb { get; set; }

  /// <summary>
  /// Медиа контент фильма
  /// </summary>
  public MediaContentModel? Content { get; set; }

  /// <summary>
  /// Список сезонов
  /// </summary>
  public List<SeasonModel>? Seasons { get; set; }

  /// <summary>
  /// Список жанров фильма
  /// </summary>
  public List<string> Genres { get; set; } = [];

  /// <summary>
  /// Список стран производства фильма
  /// </summary>
  public List<string> Countries { get; set; } = [];

  /// <summary>
  /// Список актеров фильма
  /// </summary>
  public List<Actor> Actors { get; set; } = [];

  /// <summary>
  /// Список режиссеров фильма
  /// </summary>
  public List<string> Directors { get; set; } = [];

  /// <summary>
  /// Список сценаристов фильма
  /// </summary>
  public List<string> Screenwriters { get; set; } = [];
  
  /// <summary>
  /// Дата и время изменения модели
  /// </summary>
  public DateTime ModifiedAt { get; set; }

  public FilmSnapshot GetSnapshot()
  {
    MediaContent? content = Content == null
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
        SeasonModel season = Seasons?.FirstOrDefault(ms => @as.Number == ms.Number)
                             ?? new SeasonModel { Number = @as.Number };

        season.Episodes = @as.Episodes.Select(ae =>
        {
          EpisodeModel episode = season.Episodes.FirstOrDefault(me => ae.Number == me.Number) 
                                 ?? new EpisodeModel { Number = ae.Number };

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