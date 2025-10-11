namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке создать подборку с неуникальным именем.
/// </summary>
public class PlaylistAlreadyExistsException : Exception
{
    /// <summary>
    /// Имя подборки.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="name">Имя подборки.</param>
    public PlaylistAlreadyExistsException(string name) : base($"There can't be more then one playlist with the name {name}")
    {
        Name = name;
    }
}