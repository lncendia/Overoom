using System.Security.Claims;
using Identix.Application.Abstractions;
using Identix.Infrastructure.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера Yandex
/// </summary>
public class YandexClaimsMapper() : ExternalClaimsMapperBase(OpenIddictClientWebIntegrationConstants.Providers.Yandex)
{
    /// <summary>
    /// Выполняет маппинг claims из результата аутентификации Yandex
    /// </summary>
    /// <param name="result">Результат аутентификации Yandex</param>
    /// <returns>ClaimsIdentity с маппированными claims Yandex</returns>
    /// <exception cref="Exception">Когда отсутствует обязательный идентификатор пользователя</exception>
    public override Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
    {
        // Получаем обязательный идентификатор пользователя из Yandex
        var id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Создаем базовую identity с идентификатором
        var identity = CreateBaseIdentity(id);

        // Маппим отображаемое имя пользователя
        identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue(ClaimTypes.Name));

        // Маппим email пользователя
        identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue(ClaimTypes.Email));

        // Дополнительная обработка аватара пользователя
        var avatarId = result.Principal?.GetClaim("default_avatar_id");
        if (avatarId != null)
        {
            // Формируем URL аватара по шаблону Yandex
            var url = $"https://avatars.yandex.net/get-yapic/{avatarId}/islands-75";
            identity.TryAddClaim(Constants.Claims.Thumbnail, url);
        }

        return Task.FromResult(identity);
    }
}