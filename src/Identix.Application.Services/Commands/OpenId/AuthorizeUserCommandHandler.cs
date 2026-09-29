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
    AppUser user = await userManager.FindByIdAsync(request.UserId.ToString()) ??
                   throw new UserNotFoundException();

    object application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) ??
                         throw new InvalidOperationException(
                           "The details of the calling client application could not be found");

    object? authorization = await authorizationManager.FindAsync(
        subject: await userManager.GetUserIdAsync(user),
        client: await applicationManager.GetIdAsync(application, cancellationToken),
        status: OpenIddictConstants.Statuses.Valid,
        type: OpenIddictConstants.AuthorizationTypes.Permanent,
        scopes: request.Scopes, cancellationToken: cancellationToken)
      .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    string? consentType = await applicationManager.GetConsentTypeAsync(application, cancellationToken);

    switch (consentType)
    {
      case null:
      case OpenIddictConstants.ConsentTypes.Implicit:
      case OpenIddictConstants.ConsentTypes.External when authorization is not null:
      case OpenIddictConstants.ConsentTypes.Explicit when authorization is not null:
        ClaimsIdentity identity = await claimsIdentityFactory.CreateAsync(user, request.AuthenticationScheme, request.Identity);

        List<string> resources = await scopeManager.ListResourcesAsync(request.Scopes, cancellationToken)
          .ToListAsync(cancellationToken: cancellationToken);

        identity.SetScopes(request.Scopes);
        identity.SetResources(resources);

        // Автоматически создаем постоянную авторизацию, чтобы избежать запроса явного согласия
        // для будущих запросов авторизации или токенов с теми же областями
        authorization ??= await authorizationManager.CreateAsync(
          identity: identity,
          subject: await userManager.GetUserIdAsync(user),
          client: (await applicationManager.GetIdAsync(application, cancellationToken))!,
          type: OpenIddictConstants.AuthorizationTypes.Permanent,
          scopes: identity.GetScopes(), cancellationToken: cancellationToken);

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization, cancellationToken));
        identity.SetDestinations(claimsIdentityFactory.GetDestinations);

        return new ClaimsPrincipal(identity);
    }

    throw new ConsentRequiredException(consentType);
  }
}