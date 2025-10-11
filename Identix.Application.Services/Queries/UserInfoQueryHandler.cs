using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса для получения информации о пользователе в соответствии со спецификацией OIDC UserInfo endpoint.
/// Предоставляет claims о пользователе на основе запрошенных scope'ов аутентификации.
/// </summary>
/// <param name="userManager">Менеджер пользователей ASP.NET Core Identity для управления данными пользователей</param>
public class UserInfoQueryHandler(UserManager<AppUser> userManager)
    : IRequestHandler<UserInfoQuery, UserInfoDto>
{
    /// <summary>
    /// Обрабатывает запрос на получение информации о пользователе.
    /// Возвращает только те claims, которые разрешены запрошенными scope'ами в соответствии со стандартом OpenID Connect.
    /// </summary>
    /// <param name="request">Запрос с идентификатором пользователя, запрашиваемыми scope'ами и информацией о principal</param>
    /// <param name="cancellationToken">Токен отмены операции для асинхронной обработки</param>
    /// <returns>DTO с информацией о пользователе в формате OIDC UserInfo endpoint</returns>
    /// <exception cref="UserNotFoundException">Вызывается когда пользователь с указанным ID не найден в системе</exception>
    public async Task<UserInfoDto> Handle(UserInfoQuery request, CancellationToken cancellationToken)
    {
        // Получаем профиль пользователя по его ID из базы данных
        var user = await userManager.FindByIdAsync(request.UserId.ToString()) ??
                   throw new UserNotFoundException();

        // Инициализируем DTO с обязательным claim 'sub' (subject identifier)
        var dto = new UserInfoDto
        {
            Sub = await userManager.GetUserIdAsync(user),
            Username = await userManager.GetUserNameAsync(user),
            PreferredUsername = await userManager.GetUserNameAsync(user)
        };

        // Обработка scope'а email: предоставляем email и статус подтверждения
        if (request.Scopes.Contains(OpenIddictConstants.Scopes.Email) && userManager.SupportsUserEmail)
        {
            dto.Email = await userManager.GetEmailAsync(user);
            dto.EmailVerified = await userManager.IsEmailConfirmedAsync(user);
        }

        // Обработка scope'а phone: предоставляем номер телефона и статус подтверждения
        if (request.Scopes.Contains(OpenIddictConstants.Scopes.Phone) && userManager.SupportsUserPhoneNumber)
        {
            dto.PhoneNumber = await userManager.GetPhoneNumberAsync(user);
            dto.PhoneNumberVerified = await userManager.IsPhoneNumberConfirmedAsync(user);
        }

        // Обработка scope'а roles: предоставляем список ролей пользователя
        if (request.Scopes.Contains(OpenIddictConstants.Scopes.Roles))
        {
            dto.Roles = await userManager.GetRolesAsync(user);
        }

        // Обработка scope'а profile: предоставляем основную информацию профиля
        if (request.Scopes.Contains(OpenIddictConstants.Scopes.Profile))
        {
            // Ключ или URL фотографии пользователя
            dto.Picture = user.PhotoKey;
            
            // Локаль пользователя в строковом формате
            dto.Locale = user.Locale.GetLocalizationString();
        }

        return dto;
    }
}