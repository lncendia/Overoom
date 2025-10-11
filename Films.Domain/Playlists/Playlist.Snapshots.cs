using System.Reflection;
using Films.Domain.Playlists.Snapshots;

namespace Films.Domain.Playlists;

public partial class Playlist
{
    internal static Playlist FromSnapshot(PlaylistSnapshot snapshot)
    {
        var type = typeof(Playlist);
        var ctor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(PlaylistSnapshot)],
            null);

        return (Playlist)ctor!.Invoke([snapshot]);
    }

    internal PlaylistSnapshot GetSnapshot() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        PosterKey = PosterKey,
        UpdatedAt = UpdatedAt,
        Films = _films.ToList(),
        Genres = _genres.ToList()
    };

    // Приватный конструктор для гидратации
    // ReSharper disable once UnusedMember.Local
    private Playlist(PlaylistSnapshot snapshot) : this(snapshot.Id)
    {
        Name = snapshot.Name;
        Description = snapshot.Description;
        PosterKey = snapshot.PosterKey;
        UpdatedAt = snapshot.UpdatedAt;
        _films = snapshot.Films.ToHashSet();
        _genres = snapshot.Genres.ToHashSet();
    }
}