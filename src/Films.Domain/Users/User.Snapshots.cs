using System.Diagnostics.CodeAnalysis;

using Common.Domain.Aggregates;
using Films.Domain.Users.Snapshots;

namespace Films.Domain.Users;

public partial class User : ISnapshotable<User, UserSnapshot>
{
  /// <inheritdoc/>
  static User ISnapshotable<User, UserSnapshot>.Restore(UserSnapshot snapshot)
  {
    return new User(snapshot);
  }

  /// <inheritdoc/>
  UserSnapshot ISnapshotable<User, UserSnapshot>.ToSnapshot()
  {
    return new UserSnapshot
    {
      Id = Id,
      Username = Username,
      PhotoKey = PhotoKey,
      RoomSettings = RoomSettings,
      Watchlist = [.. _watchlist],
      History = [.. _history]
    };
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  [SetsRequiredMembers]
  private User(UserSnapshot snapshot) : this(snapshot.Id)
  {
    Username = snapshot.Username;
    PhotoKey = snapshot.PhotoKey;
    RoomSettings = snapshot.RoomSettings;
    _watchlist = [.. snapshot.Watchlist];
    _history = [.. snapshot.History];
  }
}
