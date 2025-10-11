using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Commands.OpenId;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;
using Identix.Application.Abstractions.Services;

namespace Identix.Application.Services.Commands.OpenId;

/// <summary>
/// Обработчик команды авторизации пользователя через OpenID Connect
/// </summary>
/// <param name="userManager">Менеджер пользователей</param>
/// <param name="authorizationManager">Менеджер авторизаций</param>
/// <param name="applicationManager">Менеджер приложений</param>
/// <param name="claimsIdentityFactory">Фабрика для создания identity с claims</param>
/// <param name="scopeManager">Менеджер областей (scopes)</param>
public class GrantConsentCommandHandler(
    UserManager<AppUser> userManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictApplicationManager applicationManager,
    IOpenIdClaimsIdentityFactory claimsIdentityFactory,
    IOpenIddictScopeManager scopeManager) : IRequestHandler<GrantConsentCommand, ClaimsPrincipal>
{
    /// <summary>
    /// Обрабатывает команду авторизации пользователя
    /// </summary>
    /// <param name="request">Команда авторизации пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>ClaimsPrincipal с identity пользователя</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается когда пользователь или приложение не найдены</exception>
    public async Task<ClaimsPrincipal> Handle(GrantConsentCommand request, CancellationToken cancellationToken)
    {
        // Получаем профиль вошедшего в систему пользователя по его ID
        var user = await userManager.FindByIdAsync(request.UserId.ToString()) ??
                   throw new UserNotFoundException();

        // Получаем детали клиентского приложения из базы данных по client_id
        var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) ??
                          throw new InvalidOperationException(
                              "The details of the calling client application could not be found");

        // Ищем постоянные авторизации, связанные с пользователем и клиентским приложением
        var authorization = await authorizationManager.FindAsync(
                subject: await userManager.GetUserIdAsync(user),
                client: await applicationManager.GetIdAsync(application, cancellationToken),
                status: OpenIddictConstants.Statuses.Valid,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: request.Scopes, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        // Создаем identity для пользователя с указанной схемой аутентификации
        var identity = await claimsIdentityFactory.CreateAsync(user, request.AuthenticationScheme, request.Identity);

        // Получаем ресурсы запрашиваемых областей
        var resources = await scopeManager.ListResourcesAsync(request.Scopes, cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        // Устанавливаем запрошенные области (scopes) для identity
        identity.SetScopes(request.Scopes);

        // Устанавливаем ресурсы, соответствующие запрошенным областям
        identity.SetResources(resources);

        // Автоматически создаем постоянную авторизацию, чтобы избежать запроса явного согласия
        // для будущих запросов авторизации или токенов с теми же областями
        if (authorization == null && request.RememberConsent)
        {
            authorization = await authorizationManager.CreateAsync(
                identity: identity,
                subject: await userManager.GetUserIdAsync(user),
                client: (await applicationManager.GetIdAsync(application, cancellationToken))!,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: identity.GetScopes(), cancellationToken: cancellationToken);
        }

        // Обновляем описание авторизации, если оно предоставлено в запросе
        if (authorization != null && request.Description != null)
        {
            // Создание дескриптора для обновления авторизации
            var descriptor = new OpenIddictAuthorizationDescriptor();
    
            // Заполнение дескриптора текущими данными авторизации
            await authorizationManager.PopulateAsync(descriptor, authorization, cancellationToken);
    
            // Обновление описания авторизации
            descriptor.SetDescription(request.Description);

            // Применяем изменения к существующей авторизации
            await authorizationManager.UpdateAsync(authorization, descriptor, cancellationToken);
        }

        // Устанавливаем идентификатор авторизации в claims identity
        if (authorization != null)
            identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization, cancellationToken));

        // Устанавливаем destinations для claims (куда они могут быть включены)
        identity.SetDestinations(claimsIdentityFactory.GetDestinations);

        // Возвращаем ClaimsPrincipal с созданной identity
        return new ClaimsPrincipal(identity);
    }
}