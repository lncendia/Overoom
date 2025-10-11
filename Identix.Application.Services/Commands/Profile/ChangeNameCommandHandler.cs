using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Profile;

/// <summary>
/// Обработчик для смены имени у пользователя
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class ChangeNameCommandHandler(UserManager<AppUser> userManager, IPublishEndpoint publishEndpoint) : IRequestHandler<ChangeNameCommand, AppUser>
{
    /// <summary>
    /// Метод обработки команды изменения имени пользователя.
    /// </summary>
    /// <param name="request">Запрос на смену имени у пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Возвращает обновленного пользователя.</returns>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
    /// <exception cref="UserNameLengthException">Вызывается, если имя пользователя имеет некорректную длину.</exception>
    public async Task<AppUser> Handle(ChangeNameCommand request, CancellationToken cancellationToken)
    {
        // Поиск пользователя по идентификатору
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        // Вызываем исключение UserNotFoundException если не найден пользователь
        if (user == null) throw new UserNotFoundException();

        // Попытка изменения электронной имени пользователя.
        var result = await userManager.SetUserNameAsync(user, request.Name);

        // Если результат неудачный
        if (!result.Succeeded)
        {
            // Если хоть одна ошибка InvalidUserNameLength, то вызываем исключение 
            if (result.Errors.Any(error => error.Code == "InvalidUserNameLength")) throw new UserNameLengthException();
        }
        
        // Публикуем событие
        await publishEndpoint.Publish(new UserInfoChangedIntegrationEvent
        {
            Id = user.Id,
            PhotoKey = user.PhotoKey,
            Name = user.UserName!,
            Email = user.Email!,
            Locale = user.Locale.ToString()
        }, cancellationToken);

        // Возвращаем пользователя 
        return user;
    }
}