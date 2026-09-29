using System.Security.Claims;
using Identix.Application.Abstractions;
using Identix.Infrastructure.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера Google
/// </summary>
public class GoogleClaimsMapper() : ExternalClaimsMapperBase(OpenIddictClientWebIntegrationConstants.Providers.Google)
{
  /// <summary>
  /// Выполняет маппинг claims из результата аутентификации Google
  /// </summary>
  /// <param name="result">Результат аутентификации Google</param>
  /// <returns>ClaimsIdentity с маппированными claims Google</returns>
  /// <exception cref="Exception">Когда отсутствует обязательный идентификатор пользователя</exception>
  public override Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
  {
    string? id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    ClaimsIdentity identity = CreateBaseIdentity(id);
    identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue(ClaimTypes.Name));
    identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue(ClaimTypes.Email));
    identity.TryAddClaim(Constants.Claims.Thumbnail, result.Principal?.FindFirstValue("picture"));

    return Task.FromResult(identity);
  }
}