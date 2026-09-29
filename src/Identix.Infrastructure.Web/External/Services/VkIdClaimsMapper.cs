using System.Security.Claims;

using Identix.Application.Abstractions;
using Identix.Infrastructure.Web.Extensions;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера VkId
/// </summary>
public class VkIdClaimsMapper() : ExternalClaimsMapperBase(OpenIddictClientWebIntegrationConstants.Providers.VkId)
{
  /// <summary>
  /// Основной метод маппинга claims из VK в стандартные claims системы
  /// </summary>
  /// <param name="result">Результат аутентификации от VK</param>
  /// <returns>Identity с маппированными claims</returns>
  /// <exception cref="Exception">Выбрасывается при отсутствии обязательных данных</exception>
  /// <exception cref="HttpRequestException">Выбрасывается при ошибке запроса к VK API</exception>
  public override Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
  {
    string id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("VkId: missing id");

    ClaimsIdentity identity = CreateBaseIdentity(id);
    identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue(ClaimTypes.Email));
    identity.TryAddClaim(Constants.Claims.Thumbnail, result.Principal?.FindFirstValue("avatar"));
    identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue(ClaimTypes.Name));
    return Task.FromResult(identity);
  }
}
