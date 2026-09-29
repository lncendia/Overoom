using System.Security.Claims;
using Identix.Application.Abstractions.Commands.OpenId;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace Identix.Application.Services.Commands.OpenId;

/// <summary>
/// Обработчик команды авторизации клиентского приложения в OpenID Connect
/// Создает ClaimsPrincipal для клиентских учетных данных (Client Credentials flow)
/// </summary>
/// <param name="applicationManager">Менеджер для работы с клиентскими приложениями</param>
/// <param name="scopeManager">Менеджер для работы с scope'ами доступа</param>
public sealed class AuthorizeClientCommandHandler(
  IOpenIddictApplicationManager applicationManager,
  IOpenIddictScopeManager scopeManager)
  : IRequestHandler<AuthorizeClientCommand, ClaimsPrincipal>
{
  /// <summary>
  /// Обрабатывает команду авторизации клиентского приложения
  /// </summary>
  /// <param name="request">Команда авторизации клиента</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>ClaimsPrincipal с claims клиентского приложения</returns>
  /// <exception cref="InvalidOperationException">
  /// Выбрасывается когда клиентское приложение не найдено по указанному ClientId
  /// </exception>
  public async Task<ClaimsPrincipal> Handle(AuthorizeClientCommand request, CancellationToken cancellationToken)
  {
    object application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken)
                         ?? throw new InvalidOperationException(
                           "The details of the calling client application could not be found");

    var identity = new ClaimsIdentity(
      authenticationType: TokenValidationParameters.DefaultAuthenticationType,
      nameType: OpenIddictConstants.Claims.Name,
      roleType: OpenIddictConstants.Claims.Role);

    identity.SetClaim(OpenIddictConstants.Claims.Subject,
      await applicationManager.GetClientIdAsync(application, cancellationToken));

    identity.SetClaim(OpenIddictConstants.Claims.Name,
      await applicationManager.GetDisplayNameAsync(application, cancellationToken));

    List<string> resources = await scopeManager.ListResourcesAsync(identity.GetScopes(), cancellationToken)
      .ToListAsync(cancellationToken: cancellationToken);

    identity.SetScopes(request.Scopes);
    identity.SetResources(resources);
    identity.SetDestinations(GetDestinations);

    return new ClaimsPrincipal(identity);
  }

  /// <summary>
  /// Определяет в какие токены должны включаться различные claims
  /// </summary>
  /// <param name="claim">Claim для определения destination</param>
  /// <returns>Список destinations для claim</returns>
  private static IEnumerable<string> GetDestinations(Claim claim)
  {
    switch (claim.Type)
    {
      case OpenIddictConstants.Claims.Subject:
        yield return OpenIddictConstants.Destinations.AccessToken;
        break;

      case OpenIddictConstants.Claims.Name:
        yield return OpenIddictConstants.Destinations.AccessToken;
        yield return OpenIddictConstants.Destinations.IdentityToken;
        break;
    }
  }
}