using Common.Domain.Aggregates;
using Films.Domain.Films;
using Films.Domain.Users;

namespace Films.Domain.Ratings;

/// <summary>
/// Класс, представляющий оценку фильма от пользователя.
/// </summary>
public partial class Rating : AggregateRoot
{
    /// <summary>
    /// Оценка фильма.
    /// </summary>
    private double _score;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="Rating"/>.
    /// </summary>
    /// <param name="id">Идентификатор оценки.</param>
    /// <param name="film">Фильм, который пользователь оценивает.</param>
    /// <param name="user">Пользователь, который оставляет оценку.</param>
    /// <param name="score">Оценка фильма.</param>
    /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если оценка находится вне диапазона от 0 до 10.</exception>
    public Rating(Guid id, Film film, User user, double score) : base(id)
    {
        FilmId = film.Id;
        UserId = user.Id;
        Score = score;
    }

    /// <summary>
    /// Идентификатор фильма, к которому относится оценка.
    /// </summary>
    public Guid FilmId { get; }

    /// <summary>
    /// Идентификатор пользователя, оставившего оценку.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Оценка фильма.
    /// </summary>
    public double Score
    {
        get => _score;
        set
        {
            if (value is < 0 or > 10) throw new ArgumentOutOfRangeException(nameof(value));
            _score = value;
        }
    }

    /// <summary>
    /// Дата оценки.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}