using Common.Domain.Aggregates;
using Common.Domain.Extensions;

namespace Films.Domain.Playlists;

/// <summary>
/// Плейлист фильмов.
/// </summary>
public partial class Playlist(Guid id) : AggregateRoot(id)
{
    private readonly string _name = null!;
    
    private HashSet<Guid> _films = [];
    private HashSet<string> _genres = [];

    /// <summary>
    /// Название плейлиста.
    /// </summary>
    public required string Name
    {
        get => _name;
        init
        {
            value.ValidateLength(nameof(Name), 200);
            _name = value;
        }
    }

    private string _description = null!;

    /// <summary>
    /// Описание плейлиста.
    /// </summary>
    public required string Description
    {
        get => _description;
        set
        {
            value.ValidateLength(nameof(Description), 500);
            _description = value;
        }
    }

    /// <summary>
    /// URL-адрес постера плейлиста.
    /// </summary>
    public required string PosterKey { get; set; }

    /// <summary>
    /// Дата обновления плейлиста.
    /// </summary>
    public DateTime UpdatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Коллекция содержащая идентификаторы фильмов в плейлисте.
    /// </summary>
    public IReadOnlyCollection<Guid> Films => _films;

    /// <summary>
    /// Коллекция, содержащая жанры плейлиста.
    /// </summary>
    public IReadOnlyCollection<string> Genres => _genres;

    /// <summary>
    /// Обновляет список фильмов в плейлисте.
    /// </summary>
    /// <param name="films">Коллекция идентификаторов фильмов, которые нужно добавить или удалить из плейлиста.</param>
    public void UpdateFilms(IReadOnlyList<FilmToUpdate> films)
    {
        _films = films.Select(x => x.Id).ToHashSet();

        _genres = films
            .SelectMany(x => x.Genres)
            .GroupBy(g => g)
            .OrderByDescending(genre => genre.Count())
            .Select(x => x.Key)
            .Take(5)
            .ToHashSet();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Genres"></param>
    public record FilmToUpdate(Guid Id, string[] Genres);
}