using System.Reflection;
using Films.Domain.Films.Snapshots;

namespace Films.Domain.Films;

public partial class Film
{
    internal static Film FromSnapshot(FilmSnapshot snapshot)
    {
        // Получаем тип Film
        var filmType = typeof(Film);
    
        // Получаем внутренний конструктор, который принимает FilmSnapshot
        var constructor = filmType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(FilmSnapshot)],
            null);
    
        // Вызываем конструктор и возвращаем результат
        return (Film)constructor!.Invoke([snapshot]);
    }
    
    /// <summary>
    /// Внутренний конструктор для гидратации из снапшота или БД.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Film(FilmSnapshot snapshot) : this(snapshot.Id)
    {
        _title = snapshot.Title;
        _description = snapshot.Description;
        _shortDescription = snapshot.ShortDescription;
        Date = snapshot.Date;
        PosterKey = snapshot.PosterKey;
        RatingKp = snapshot.RatingKp;
        RatingImdb = snapshot.RatingImdb;
        Content = snapshot.Content;
        Seasons = snapshot.Seasons;
        _genres = snapshot.Genres.ToHashSet();
        _countries = snapshot.Countries.ToHashSet();
        _actors = snapshot.Actors.ToHashSet();
        _directors = snapshot.Directors.ToHashSet();
        _screenwriters = snapshot.Screenwriters.ToHashSet();
    }

    /// <summary>
    /// Получение снапшота текущего состояния агрегата.
    /// </summary>
    internal FilmSnapshot GetSnapshot() => new()
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
