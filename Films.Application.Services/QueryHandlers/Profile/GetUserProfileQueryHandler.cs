using Films.Application.Abstractions.DTOs.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Application.Abstractions.Queries.Profile;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Profile;

/// <summary>
/// Обработчик запроса для получения профиля пользователя
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetUserProfileQueryHandler(MongoDbContext context)
    : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    /// <summary>
    /// Обрабатывает запрос на получение профиля пользователя
    /// </summary>
    /// <param name="request">Запрос, содержащий идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO профиля пользователя</returns>
    /// <exception cref="UserNotFoundException">Выбрасывается, если пользователь с указанным ID не найден</exception>
    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        // Находим первого пользователя с указанным ID
        var user = await context.Users.AsQueryable()
            .Where(u => u.Id == request.Id)
            .Select(u => new UserProfileDto
            {
                UserName = u.Username,
                PhotoKey = u.PhotoKey,
                RoomSettings = u.RoomSettings,
                Genres = u.Genres
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        // Возвращаем найденного пользователя или выбрасываем исключение
        return user ?? throw new UserNotFoundException(request.Id);
    }
}