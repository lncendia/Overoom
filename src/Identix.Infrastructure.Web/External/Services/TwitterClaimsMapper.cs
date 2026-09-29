using System.Security.Claims;
using Identix.Application.Abstractions;
using Identix.Infrastructure.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера Twitter
/// </summary>
public class TwitterClaimsMapper() : ExternalClaimsMapperBase(OpenIddictClientWebIntegrationConstants.Providers.Twitter)
{
  /// <summary>
  /// Выполняет маппинг claims из результата аутентификации Twitter
  /// </summary>
  /// <param name="result">Результат аутентификации Twitter</param>
  /// <returns>ClaimsIdentity с маппированными claims Twitter</returns>
  /// <exception cref="Exception">Когда отсутствует обязательный идентификатор пользователя</exception>
  public override Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
  {
    string? id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    ClaimsIdentity identity = CreateBaseIdentity(id);
    identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue("username"));
    identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue("confirmed_email"));
    identity.TryAddClaim(Constants.Claims.Thumbnail, result.Principal?.FindFirstValue("profile_image_url"));

    return Task.FromResult(identity);
  }
}