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
        // Получаем обязательный идентификатор пользователя из Discord
        var id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Создаем базовую identity с идентификатором
        var identity = CreateBaseIdentity(id);

        // Маппим отображаемое имя пользователя
        identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue(ClaimTypes.Name));

        // Маппим email пользователя
        identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue(ClaimTypes.Email));

        // Дополнительная обработка аватара пользователя
        var avatarId = result.Principal?.GetClaim("avatar");
        if (avatarId != null)
        {
            // Формируем URL аватара по шаблону Discord
            var url = $"https://cdn.discordapp.com/avatars/{id}/{avatarId}";
            identity.TryAddClaim(Constants.Claims.Thumbnail, url);
        }

        return Task.FromResult(identity);
    }
}