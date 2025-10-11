using System.Security.Claims;
using Identix.Infrastructure.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера Microsoft
/// </summary>
public class MicrosoftClaimsMapper() : ExternalClaimsMapperBase(OpenIddictClientWebIntegrationConstants.Providers.Microsoft)
{
    /// <summary>
    /// Выполняет маппинг claims из результата аутентификации Microsoft
    /// </summary>
    /// <param name="result">Результат аутентификации Microsoft</param>
    /// <returns>ClaimsIdentity с маппированными claims Microsoft</returns>
    /// <exception cref="Exception">Когда отсутствует обязательный идентификатор пользователя</exception>
    public override Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
    {
        // Получаем обязательный идентификатор пользователя из Microsoft
        var id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Создаем базовую identity с идентификатором
        var identity = CreateBaseIdentity(id);
        
        // Маппим имя пользователя из Microsoft
        identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue("name"));
        
        // Маппим email пользователя
        identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue(ClaimTypes.Email));
        
        return Task.FromResult(identity);
    }
}