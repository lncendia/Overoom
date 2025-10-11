namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке создать фильм с неуникальным именем и годом выпуска.
/// </summary>
public class FilmAlreadyExistsException : Exception
{
    /// <summary>
    /// Имя фильма.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Дата выпуска фильма.
    /// </summary>
    public DateOnly Date { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="date">Дата выпуска фильма.</param>
    /// <param name="name">Имя фильма.</param>
    public FilmAlreadyExistsException(string name, DateOnly date)
        : base($"There can't be more then one films with the title {name} and the {date:dd.MM.yyyy} release date")
    {
        Date = date;
        Name = name;
    }
}