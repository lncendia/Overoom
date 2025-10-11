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
/// Обработчик команды авторизации пользователя через OpenID Connect
/// </summary>
/// <param name="userManager">Менеджер пользователей</param>
/// <param name="authorizationManager">Менеджер авторизаций</param>
/// <param name="applicationManager">Менеджер приложений</param>
/// <param name="claimsIdentityFactory">Фабрика для создания identity с claims</param>
/// <param name="scopeManager">Менеджер областей (scopes)</param>
public class AuthorizeUserCommandHandler(
    UserManager<AppUser> userManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictApplicationManager applicationManager,
    IOpenIdClaimsIdentityFactory claimsIdentityFactory,
    IOpenIddictScopeManager scopeManager) : IRequestHandler<AuthorizeUserCommand, ClaimsPrincipal>
{
    /// <summary>
    /// Обрабатывает команду авторизации пользователя
    /// </summary>
    /// <param name="request">Команда авторизации пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>ClaimsPrincipal с identity пользователя</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается когда пользователь или приложение не найдены</exception>
    public async Task<ClaimsPrincipal> Handle(AuthorizeUserCommand request, CancellationToken cancellationToken)
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

        // Получаем тип подтверждения приложения
        var consentType = await applicationManager.GetConsentTypeAsync(application, cancellationToken);

        // Проверяем тип согласия (consent type) для приложения
        switch (consentType)
        {
            case null:
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when authorization is not null:
            case OpenIddictConstants.ConsentTypes.Explicit when authorization is not null:

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
                authorization ??= await authorizationManager.CreateAsync(
                    identity: identity,
                    subject: await userManager.GetUserIdAsync(user),
                    client: (await applicationManager.GetIdAsync(application, cancellationToken))!,
                    type: OpenIddictConstants.AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes(), cancellationToken: cancellationToken);

                // Устанавливаем идентификатор авторизации в claims identity
                identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization, cancellationToken));

                // Устанавливаем destinations для claims (куда они могут быть включены)
                identity.SetDestinations(claimsIdentityFactory.GetDestinations);

                // Возвращаем ClaimsPrincipal с созданной identity
                return new ClaimsPrincipal(identity);
        }

        // Выбрасываем исключение о необходимости согласия
        throw new ConsentRequiredException(consentType);
    }
}