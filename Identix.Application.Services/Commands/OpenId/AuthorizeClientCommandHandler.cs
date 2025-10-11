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
        // Поиск клиентского приложения по ClientId
        var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) 
                          ?? throw new InvalidOperationException(
                              "The details of the calling client application could not be found");

        // Создание identity для клиентского приложения
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Установка основных claims клиентского приложения
        identity.SetClaim(OpenIddictConstants.Claims.Subject,
            await applicationManager.GetClientIdAsync(application, cancellationToken));
        
        identity.SetClaim(OpenIddictConstants.Claims.Name,
            await applicationManager.GetDisplayNameAsync(application, cancellationToken));

        // Получаем ресурсы запрашиваемых областей
        var resources = await scopeManager.ListResourcesAsync(identity.GetScopes(), cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        // Устанавливаем запрошенные области (scopes) для identity
        identity.SetScopes(request.Scopes);

        // Устанавливаем ресурсы, соответствующие запрошенным областям
        identity.SetResources(resources);
        
        // Определение destinations для claims (в какие токены включать claims)
        identity.SetDestinations(GetDestinations);

        // Возвращаем ClaimsPrincipal с созданной identity
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
            // Subject claim включается только в Access Token
            case OpenIddictConstants.Claims.Subject:
                yield return OpenIddictConstants.Destinations.AccessToken;
                break;
            
            // Name claim включается в оба токена
            case OpenIddictConstants.Claims.Name:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield return OpenIddictConstants.Destinations.IdentityToken;
                break;
        }
    }
}