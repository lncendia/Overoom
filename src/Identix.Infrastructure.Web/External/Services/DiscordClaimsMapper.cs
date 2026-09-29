using System.Security.Claims;
using Identix.Application.Abstractions;
using Identix.Infrastructure.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера Discord
/// </summary>
public class DiscordClaimsMapper() : ExternalClaimsMapperBase(OpenIddictClientWebIntegrationConstants.Providers.Discord)
{
  /// <summary>
  /// Выполняет маппинг claims из результата аутентификации Discord
  /// </summary>
  /// <param name="result">Результат аутентификации Discord</param>
  /// <returns>ClaimsIdentity с маппированными claims Discord</returns>
  /// <exception cref="Exception">Когда отсутствует обязательный идентификатор пользователя</exception>
  public override Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
  {
    string? id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    ClaimsIdentity identity = CreateBaseIdentity(id);
    identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue(ClaimTypes.Name));
    identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue(ClaimTypes.Email));
    string? avatarId = result.Principal?.GetClaim("avatar");
    if (avatarId != null)
    {
      string url = $"https://cdn.discordapp.com/avatars/{id}/{avatarId}";
      identity.TryAddClaim(Constants.Claims.Thumbnail, url);
    }

    return Task.FromResult(identity);
  }
}