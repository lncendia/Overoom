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

    ClaimsIdentity identity = await claimsIdentityFactory.CreateAsync(user, request.AuthenticationScheme, request.Identity);

    List<string> resources = await scopeManager.ListResourcesAsync(request.Scopes, cancellationToken)
      .ToListAsync(cancellationToken: cancellationToken);

    identity.SetScopes(request.Scopes);
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

    if (authorization != null && request.Description != null)
    {
      var descriptor = new OpenIddictAuthorizationDescriptor();
      await authorizationManager.PopulateAsync(descriptor, authorization, cancellationToken);
      descriptor.SetDescription(request.Description);
      await authorizationManager.UpdateAsync(authorization, descriptor, cancellationToken);
    }

    if (authorization != null)
      identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization, cancellationToken));

    identity.SetDestinations(claimsIdentityFactory.GetDestinations);

    return new ClaimsPrincipal(identity);
  }
}