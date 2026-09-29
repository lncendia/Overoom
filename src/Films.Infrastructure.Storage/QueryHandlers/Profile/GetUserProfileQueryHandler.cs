using Films.Application.Abstractions.DTOs.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Application.Abstractions.Queries.Profile;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Infrastructure.Storage.QueryHandlers.Profile;

/// <summary>
/// Обработчик запроса для получения профиля пользователя
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetUserProfileQueryHandler(MongoDbContext context)
  : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
  /// <summary>
  /// Количество любимых жанров, показываемых в профиле
  /// </summary>
  private const int FavoriteGenresCount = 5;

  /// <summary>
  /// Обрабатывает запрос на получение профиля пользователя
  /// </summary>
  /// <param name="request">Запрос, содержащий идентификатор пользователя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>DTO профиля пользователя</returns>
  /// <exception cref="UserNotFoundException">Выбрасывается, если пользователь с указанным ID не найден</exception>
  public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
  {
    var user = await context.Users.AsQueryable()
      .Where(u => u.Id == request.Id)
      .Select(u => new { u.Username, u.PhotoKey, u.RoomSettings, u.GenreCounts })
      .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    if (user == null) throw new UserNotFoundException(request.Id);

    return new UserProfileDto
    {
      UserName = user.Username,
      PhotoKey = user.PhotoKey,
      RoomSettings = user.RoomSettings,
      Genres = user.GenreCounts
        .Where(g => g.Value > 0)
        .OrderByDescending(g => g.Value)
        .Take(FavoriteGenresCount)
        .Select(g => g.Key)
        .ToList()
    };
  }
}