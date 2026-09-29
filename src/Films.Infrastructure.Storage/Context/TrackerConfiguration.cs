using Films.Infrastructure.Storage.Models.CommentReactions;
using Films.Infrastructure.Storage.Models.Comments;
using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Models.Playlists;
using Films.Infrastructure.Storage.Models.Ratings;
using Films.Infrastructure.Storage.Models.Rooms;
using Films.Infrastructure.Storage.Models.Users;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Context;

/// <summary>
/// Конфигурация трекера изменений моделей хранения.
/// </summary>
public static class TrackerConfiguration
{
  /// <summary>
  /// Создаёт конфигурацию трекера: идентификаторы, версии, отслеживаемые вложенные объекты и коллекции моделей.
  /// </summary>
  /// <returns>Конфигурация трекера</returns>
  public static ModelBuilder ConfigureModelBuilder()
  {
    var builder = new ModelBuilder();
    builder.Entity<CommentModel>(e =>
    {
      e.Property(c => c.Id).IsIdentifier();
      e.Property(c => c.ModifiedAt).IsVersion();
    });

    builder.Entity<CommentReactionModel>(e =>
    {
      e.Property(r => r.Id).IsIdentifier();
      e.Property(r => r.ModifiedAt).IsVersion();
    });

    builder.Entity<FilmModel>(e =>
    {
      e.Property(f => f.Id).IsIdentifier();
      e.Property(f => f.Actors).IsSet();
      e.Property(f => f.Countries).IsSet();
      e.Property(f => f.Genres).IsSet();
      e.Property(f => f.Directors).IsSet();
      e.Property(f => f.Screenwriters).IsSet();
      e.Property(f => f.Content).IsChild();
      e.Property(f => f.Seasons).IsTrackedSet();
      e.Property(f => f.ModifiedAt).IsVersion();
    });

    builder.Entity<MediaContentModel>(e => e.Property(c => c.Versions).IsSet());
    builder.Entity<SeasonModel>(e =>
    {
      e.Property(c => c.Number).IsIdentifier();
      e.Property(c => c.Episodes).IsTrackedSet();
    });

    builder.Entity<EpisodeModel>(e =>
    {
      e.Property(c => c.Number).IsIdentifier();
      e.Property(c => c.Versions).IsSet();
    });

    builder.Entity<PlaylistModel>(e =>
    {
      e.Property(p => p.Id).IsIdentifier();
      e.Property(p => p.Films).IsSet();
      e.Property(p => p.Genres).IsSet();
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
      e.Property(r => r.Viewers).IsSet();
      e.Property(r => r.BannedUsers).IsSet();
      e.Property(r=>r.ModifiedAt).IsVersion();
    });

    builder.Entity<UserModel>(e =>
    {
      e.Property(u => u.Id).IsIdentifier();
      e.Property(u => u.Watchlist).IsSet();
      e.Property(u => u.History).IsSet();
      e.Property(u=>u.ModifiedAt).IsVersion();
    });

    return builder;
  }
}
