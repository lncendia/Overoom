namespace Films.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда не удается найти подборку.
/// </summary>
public class PlaylistNotFoundException : Exception
{
    /// <summary>
    /// Идентификатор подборки, который не был найден.
    /// </summary>
    public Guid PlaylistId { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="playlistId">Идентификатор подборки.</param>
    public PlaylistNotFoundException(Guid playlistId) : base($"Playlist with ID {playlistId} not found.")
    {
        PlaylistId = playlistId;
    }
}