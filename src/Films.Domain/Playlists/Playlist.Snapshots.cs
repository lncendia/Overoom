using System.Diagnostics.CodeAnalysis;

using Common.Domain.Aggregates;
using Films.Domain.Playlists.Snapshots;

namespace Films.Domain.Playlists;

public partial class Playlist : ISnapshotable<Playlist, PlaylistSnapshot>
{
  /// <inheritdoc/>
  static Playlist ISnapshotable<Playlist, PlaylistSnapshot>.Restore(PlaylistSnapshot snapshot)
  {
    return new Playlist(snapshot);
  }

  /// <inheritdoc/>
  PlaylistSnapshot ISnapshotable<Playlist, PlaylistSnapshot>.ToSnapshot()
  {
    return new PlaylistSnapshot
    {
      Id = Id,
      Name = Name,
      Description = Description,
      PosterKey = PosterKey,
      UpdatedAt = UpdatedAt,
      Films = [.. _films],
      Genres = [.. _genres]
    };
  }

  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  [SetsRequiredMembers]
  private Playlist(PlaylistSnapshot snapshot) : this(snapshot.Id)
  {
    Name = snapshot.Name;
    Description = snapshot.Description;
    PosterKey = snapshot.PosterKey;
    UpdatedAt = snapshot.UpdatedAt;
    _films = [.. snapshot.Films];
    _genres = [.. snapshot.Genres];
  }
}