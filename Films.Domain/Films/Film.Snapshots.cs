using System.Diagnostics.CodeAnalysis;

using Common.Domain.Aggregates;
using Films.Domain.Films.Snapshots;
using Films.Domain.Films.ValueObjects;

namespace Films.Domain.Films;

public partial class Film : ISnapshotable<Film, FilmSnapshot>
{
  /// <inheritdoc/>
  static Film ISnapshotable<Film, FilmSnapshot>.Restore(FilmSnapshot snapshot)
  {
    return new Film(snapshot);
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  [SetsRequiredMembers]
  private Film(FilmSnapshot snapshot) : this(snapshot.Id)
  {
    Date = snapshot.Date;
    PosterKey = snapshot.PosterKey;
    RatingKp = snapshot.RatingKp;
    RatingImdb = snapshot.RatingImdb;
    Content = snapshot.Content;
    ShortDescription = snapshot.ShortDescription;
    Seasons = snapshot.Seasons == null ? null : new SortedSet<Season>(snapshot.Seasons);
    Title = snapshot.Title;
    _description = snapshot.Description;
    _genres = [.. snapshot.Genres];
    _countries = [.. snapshot.Countries];
    _actors = [.. snapshot.Actors];
    _directors = [.. snapshot.Directors];
    _screenwriters = [.. snapshot.Screenwriters];
  }

  /// <inheritdoc/>
  FilmSnapshot ISnapshotable<Film, FilmSnapshot>.ToSnapshot()
  {
    return new FilmSnapshot
    {
      Id = Id,
      Title = Title,
      Description = Description,
      ShortDescription = ShortDescription,
      Date = Date,
      PosterKey = PosterKey,
      RatingKp = RatingKp,
      RatingImdb = RatingImdb,
      Content = Content,
      Seasons = Seasons,
      Genres = Genres,
      Countries = Countries,
      Actors = Actors,
      Directors = Directors,
      Screenwriters = Screenwriters
    };
  }
}
