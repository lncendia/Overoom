using Common.Application.Events;
using Common.Domain.Events;
using Common.Infrastructure.Transactions;

using Films.Domain.Ratings;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Models.Users;

using MongoDB.Driver;

namespace Films.Infrastructure.Storage.EventHandlers;

/// <summary>
/// Обновляет денормализованную статистику при появлении новой оценки: количество и сумму оценок фильма
/// и статистику жанров пользователя.
/// </summary>
/// <remarks>
/// Выполняется до сохранения, поэтому внутри транзакции точки входа $inc фиксируется или откатывается
/// вместе с самой оценкой.
/// </remarks>
/// <param name="context">MongoDB-контекст приложения</param>
/// <param name="transaction">Текущая транзакция области</param>
public class RatingCreatedStatisticsEventHandler(MongoDbContext context, ITransactionContext transaction)
  : BeforeSaveNotificationHandler<CreateEvent<Rating>>
{
  /// <inheritdoc/>
  protected override async Task Execute(CreateEvent<Rating> notification, CancellationToken cancellationToken)
  {
    Rating rating = notification.Aggregate;
    IClientSessionHandle? session = transaction.Session;

    FilterDefinition<FilmModel> filmFilter = Builders<FilmModel>.Filter.Eq(f => f.Id, rating.FilmId);

    UpdateDefinition<FilmModel> filmUpdate = Builders<FilmModel>.Update
      .Inc(f => f.UserRatingsCount, 1)
      .Inc(f => f.UserRatingsSum, rating.Score);

    var options = new FindOneAndUpdateOptions<FilmModel, FilmGenres>
    {
      Projection = Builders<FilmModel>.Projection.Include(f => f.Genres)
    };

    FilmGenres? film = session == null
      ? await context.Films.FindOneAndUpdateAsync(filmFilter, filmUpdate, options, cancellationToken)
      : await context.Films.FindOneAndUpdateAsync(session, filmFilter, filmUpdate, options, cancellationToken);

    UpdateDefinition<UserModel>[] genreUpdates = (film?.Genres ?? [])
      .Where(IsValidFieldName)
      .Distinct()
      .Select(genre => Builders<UserModel>.Update.Inc($"{nameof(UserModel.GenreCounts)}.{genre}", 1))
      .ToArray();

    if (genreUpdates.Length == 0) return;

    FilterDefinition<UserModel> userFilter = Builders<UserModel>.Filter.Eq(u => u.Id, rating.UserId);
    UpdateDefinition<UserModel> userUpdate = Builders<UserModel>.Update.Combine(genreUpdates);

    if (session == null)
      await context.Users.UpdateOneAsync(userFilter, userUpdate, cancellationToken: cancellationToken);
    else
      await context.Users.UpdateOneAsync(session, userFilter, userUpdate, cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Проверяет, что жанр можно использовать как имя поля документа
  /// </summary>
  /// <param name="genre">Название жанра</param>
  /// <returns>true, если имя поля допустимо</returns>
  private static bool IsValidFieldName(string genre)
  {
    return !string.IsNullOrWhiteSpace(genre) && !genre.Contains('.') && !genre.StartsWith('$');
  }

  /// <summary>
  /// Проекция документа фильма с жанрами
  /// </summary>
  private class FilmGenres
  {
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Жанры фильма
    /// </summary>
    public List<string> Genres { get; set; } = [];
  }
}
