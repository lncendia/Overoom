using Common.Infrastructure.Repositories.Models;

using Films.Domain.Films.Snapshots;
using Films.Domain.Films.ValueObjects;

using MongoDB.Bson.Serialization.Attributes;

namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель фильма для работы с базой данных.
/// </summary>
[BsonIgnoreExtraElements]
public class FilmModel : IModel<FilmSnapshot>
{
  #region Поля и свойства

  /// <summary>
  /// Название фильма
  /// </summary>
  public string Title { get; set; } = null!;

  /// <summary>
  /// Ссылка на постер фильма
  /// </summary>
  public string PosterKey { get; set; } = null!;

  /// <summary>
  /// Полное описание фильма
  /// </summary>
  public string Description { get; set; } = null!;

  /// <summary>
  /// Краткое описание фильма
  /// </summary>
  public string ShortDescription { get; set; } = null!;

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

  /// <summary>
  /// Количество пользовательских оценок фильма.
  /// </summary>
  /// <remarks>
  /// Денормализованный счётчик для запросов чтения. Не входит в снапшот агрегата и обновляется
  /// только атомарным $inc из <see cref="Repositories.RatingRepository"/>, поэтому трекер его не перезаписывает.
  /// </remarks>
  public int UserRatingsCount { get; set; }

  /// <summary>
  /// Сумма пользовательских оценок фильма.
  /// </summary>
  /// <remarks>
  /// Денормализованное значение для расчёта средней оценки, обновляется вместе с <see cref="UserRatingsCount"/>.
  /// </remarks>
  public double UserRatingsSum { get; set; }

  #endregion

  #region IModel

  /// <summary>
  /// Уникальный идентификатор фильма
  /// </summary>
  public required Guid Id { get; init; }

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
    Genres = [.. snapshot.Genres];
    Countries = [.. snapshot.Countries];
    Actors = [.. snapshot.Actors];
    Directors = [.. snapshot.Directors];
    Screenwriters = [.. snapshot.Screenwriters];

    if (snapshot.Content != null)
    {
      Content ??= new MediaContentModel();
      Content.Versions = [.. snapshot.Content.Versions];
    }
    else
    {
      Content = null;
    }

    if (snapshot.Seasons != null)
    {
      var seasonsByNumber = Seasons?.ToDictionary(s => s.Number);

      Seasons =
      [
        .. snapshot.Seasons.Select(@as =>
        {
          if (seasonsByNumber == null || !seasonsByNumber.TryGetValue(@as.Number, out SeasonModel? season))
            season = new SeasonModel { Number = @as.Number };

          var episodesByNumber = season.Episodes.ToDictionary(e => e.Number);

          season.Episodes =
          [
            .. @as.Episodes.Select(ae =>
            {
              if (!episodesByNumber.TryGetValue(ae.Number, out EpisodeModel? episode))
                episode = new EpisodeModel { Number = ae.Number };

              episode.Versions = [.. ae.Versions];
              return episode;
            })
          ];

          return season;
        })
      ];
    }
    else
    {
      Seasons = null;
    }
  }

  #endregion
}
