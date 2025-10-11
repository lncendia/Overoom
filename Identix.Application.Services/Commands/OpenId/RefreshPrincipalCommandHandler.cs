using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Commands.OpenId;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Services;

namespace Identix.Application.Services.Commands.OpenId;

/// <summary>
/// Обработчик команды обновления ClaimsPrincipal для refresh token flow
/// </summary>
/// <param name="userManager">Менеджер пользователей для работы с данными пользователя</param>
/// <param name="claimsIdentityFactory">Фабрика для создания identity с claims</param>
public class RefreshPrincipalCommandHandler(
    UserManager<AppUser> userManager,
    IOpenIdClaimsIdentityFactory claimsIdentityFactory) : IRequestHandler<RefreshPrincipalCommand, ClaimsPrincipal>
{
    /// <summary>
    /// Обрабатывает команду обновления principal для выдачи нового access token
    /// </summary>
    /// <param name="request">Команда обновления principal</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Обновленный ClaimsPrincipal для создания нового access token</returns>
    /// <exception cref="UserNotFoundException">Выбрасывается когда пользователь не найден</exception>
    public async Task<ClaimsPrincipal> Handle(RefreshPrincipalCommand request, CancellationToken cancellationToken)
    {
        // Получаем профиль пользователя по его ID
        var user = await userManager.FindByIdAsync(request.UserId.ToString()) ??
                   throw new UserNotFoundException();

        // Создаем обновленную identity для пользователя на основе существующего principal
        var identity = await claimsIdentityFactory.CreateAsync(user, request.AuthenticationScheme, request.Identity);

        // Устанавливаем destinations для claims - определяем в какие токены будут включены claims
        identity.SetDestinations(claimsIdentityFactory.GetDestinations);

        // Возвращаем новый ClaimsPrincipal с обновленной identity
        return new ClaimsPrincipal(identity);
    }
}