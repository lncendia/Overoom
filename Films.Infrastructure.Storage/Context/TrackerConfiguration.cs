using Films.Infrastructure.Storage.Models.Comments;
using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Models.Playlists;
using Films.Infrastructure.Storage.Models.Ratings;
using Films.Infrastructure.Storage.Models.Rooms;
using Films.Infrastructure.Storage.Models.Users;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Context;

/// <summary>
///
/// </summary>
public class TrackerConfiguration
{
  /// <summary>
  ///
  /// </summary>
  /// <returns></returns>
  public static ModelBuilder ConfigureModelBuilder()
  {
    var builder = new ModelBuilder();
    builder.Entity<CommentModel>(e =>
    {
      e.Property(c => c.Id).IsIdentifier();
      e.Property(c => c.ModifiedAt).IsVersion();
    });

    builder.Entity<FilmModel>(e =>
    {
      e.Property(f => f.Id).IsIdentifier();
      e.Property(f => f.Actors).IsCollection();
      e.Property(f => f.Countries).IsCollection();
      e.Property(f => f.Genres).IsCollection();
      e.Property(f => f.Directors).IsCollection();
      e.Property(f => f.Screenwriters).IsCollection();
      e.Property(f => f.Content).IsTrackedObject();
      e.Property(f => f.Seasons).IsTrackedObjectCollection();
      e.Property(f => f.ModifiedAt).IsVersion();
    });

    builder.Entity<MediaContentModel>(e => e.Property(c => c.Versions).IsCollection());
    builder.Entity<SeasonModel>(e => e.Property(c => c.Episodes).IsTrackedObjectCollection());
    builder.Entity<EpisodeModel>(e => e.Property(c => c.Versions).IsCollection());

    builder.Entity<PlaylistModel>(e =>
    {
      e.Property(p => p.Id).IsIdentifier();
      e.Property(p => p.Films).IsCollection();
      e.Property(p => p.Genres).IsCollection();
      e.Property(p => p.UpdatedAt).IsVersion();
    });

    builder.Entity<RatingModel>(e =>
    {
      e.Property(r => r.Id).IsIdentifier();
      e.Property(p => p.ModifiedAt).IsVersion();
    });

    builder.Entity<RoomModel>(e =>
    {
      e.Property(r => r.Id).IsIdentifier();
      e.Property(r => r.Viewers).IsCollection();
      e.Property(r => r.BannedUsers).IsCollection();
      e.Property(r=>r.ModifiedAt).IsVersion();
    });

    builder.Entity<UserModel>(e =>
    {
      e.Property(u => u.Id).IsIdentifier();
      e.Property(u => u.Watchlist).IsCollection();
      e.Property(u => u.History).IsCollection();
      e.Property(u => u.Genres).IsCollection();
      e.Property(u=>u.ModifiedAt).IsVersion();
    });

    return builder;
  }
}
