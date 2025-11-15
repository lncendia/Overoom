using System.Reflection;
using Films.Domain.Users.Snapshots;

namespace Films.Domain.Users;

public partial class User
{
    internal static User FromSnapshot(UserSnapshot snapshot)
    {
        var type = typeof(User);
        var ctor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(UserSnapshot)],
            null);

        return (User)ctor!.Invoke([snapshot]);
    }

    internal UserSnapshot GetSnapshot() => new()
    {
        Id = Id,
        Username = Username,
        PhotoKey = PhotoKey,
        RoomSettings = RoomSettings,
        Watchlist = _watchlist.ToList(),
        History = _history.ToList(),
        Genres = _genres.ToList()
    };

    // Приватный конструктор для гидратации
    // ReSharper disable once UnusedMember.Local
    private User(UserSnapshot snapshot) : this(snapshot.Id)
    {
        Username = snapshot.Username;
        PhotoKey = snapshot.PhotoKey;
        RoomSettings = snapshot.RoomSettings;
        _watchlist = snapshot.Watchlist.ToHashSet();
        _history = snapshot.History.ToHashSet();
        _genres = snapshot.Genres.ToHashSet();
    }
}