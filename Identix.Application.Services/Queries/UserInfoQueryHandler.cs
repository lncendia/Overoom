using System.Security.Claims;

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
    AppUser user = await userManager.FindByIdAsync(request.UserId.ToString()) ??
                   throw new UserNotFoundException();

    IList<Claim> claims = await userManager.GetClaimsAsync(user);

    var dto = new UserInfoDto
    {
      Sub = await userManager.GetUserIdAsync(user),
      Username = await userManager.GetUserNameAsync(user),
      PreferredUsername = await userManager.GetUserNameAsync(user)
    };

    if (request.Scopes.Contains(OpenIddictConstants.Scopes.Email) && userManager.SupportsUserEmail)
    {
      dto.Email = await userManager.GetEmailAsync(user);
      dto.EmailVerified = await userManager.IsEmailConfirmedAsync(user);
    }

    if (request.Scopes.Contains(OpenIddictConstants.Scopes.Phone) && userManager.SupportsUserPhoneNumber)
    {
      dto.PhoneNumber = await userManager.GetPhoneNumberAsync(user);
      dto.PhoneNumberVerified = await userManager.IsPhoneNumberConfirmedAsync(user);
    }

    if (request.Scopes.Contains(OpenIddictConstants.Scopes.Roles))
    {
      dto.Roles = await userManager.GetRolesAsync(user);
    }

    if (request.Scopes.Contains(OpenIddictConstants.Scopes.Profile))
    {
      dto.Picture = AppUser.GetPhotoKey(claims);
      dto.Locale = AppUser.GetLocale(claims).GetLocalizationString();
    }

    return dto;
  }
}
