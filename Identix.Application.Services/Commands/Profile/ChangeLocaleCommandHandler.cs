using System.Security.Claims;
using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;

namespace Identix.Application.Services.Commands.Profile;

/// <summary>
/// Обработчик для смены локали у пользователя
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class ChangeLocaleCommandHandler(UserManager<AppUser> userManager, IPublishEndpoint publishEndpoint)
    : IRequestHandler<ChangeLocaleCommand>
{
    /// <summary>
    /// Метод обработки команды изменения локали пользователя.
    /// </summary>
    /// <param name="request">Запрос на смену локали у пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Возвращает обновленного пользователя.</returns>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
    /// <exception cref="UserNameLengthException">Вызывается, если имя пользователя имеет некорректную длину.</exception>
    public async Task Handle(ChangeLocaleCommand request, CancellationToken cancellationToken)
    {
        // Поиск пользователя по идентификатору
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        // Вызываем исключение UserNotFoundException если не найден пользователь
        if (user == null) throw new UserNotFoundException();

        // Сохраняем старый клайм
        var oldClaim = user.Claims.FirstOrDefault(c => c.ClaimType == OpenIddictConstants.Claims.Locale)?.ToClaim();

        // Создаем новый клайм
        var newClaim = new Claim(OpenIddictConstants.Claims.Locale, user.Locale.GetLocalizationString());

        // Если старый клайм существует
        if (oldClaim != null)
        {
            // Заменяем клайм аватара
            await userManager.ReplaceClaimAsync(user, oldClaim, newClaim);
        }
        else
        {
            // Добавляем утверждение об аватаре
            await userManager.AddClaimAsync(user, newClaim);
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
    }
}