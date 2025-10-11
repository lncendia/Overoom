using System.Security.Claims;
using Identix.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace Identix.Infrastructure.Web.Account.Services;

/// <inheritdoc/>
public class OpenIdSignInManager<TUser>(
    UserManager<TUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<TUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<TUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<TUser> confirmation)
    : SignInManager<TUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    where TUser : class
{
    /// <inheritdoc/>
    public override async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties? authenticationProperties, IEnumerable<Claim> additionalClaims)
    {
        // Создание основного principal для пользователя
        var userPrincipal = await CreateUserPrincipalAsync(user);
        
        // Добавление дополнительных claims к identity
        foreach (var claim in additionalClaims)
        {
            userPrincipal.Identities.First().AddClaim(claim);
        }
        
        // Дополнение principal недостающими claims
        AugmentMissingClaims(userPrincipal);
        
        // Выполнение операции входа в систему
        await Context.SignInAsync(
            AuthenticationScheme,
            userPrincipal,
            authenticationProperties ?? new AuthenticationProperties());

        // Обновление контекста пользователя для немедленного применения claims
        Context.User = userPrincipal;
    }
        
    /// <summary>
    /// Дополняет объект ClaimsPrincipal недостающими claims (утверждениями) для корректной работы с OpenIddict.
    /// Выполняет преобразование и добавление стандартных утверждений, необходимых для аутентификации и авторизации.
    /// </summary>
    /// <param name="principal">Объект ClaimsPrincipal, который требуется дополнить утверждениями</param>
    private static void AugmentMissingClaims(ClaimsPrincipal principal)
    {
        // Получаем первую identity из principal (обычно используется одна identity)
        var identity = principal.Identities.First();

        // Обработка claim метода аутентификации (AuthenticationMethod)
        // ASP.NET Identity использует этот тип claim с именем провайдера аутентификации (например, "Google")
        // Этот код преобразует его в стандартные claims для нашего сценария
        var amr = identity.FindFirst(ClaimTypes.AuthenticationMethod);

        // Если найден claim метода аутентификации И отсутствуют стандартные claims провайдера и метода аутентификации
        if (amr != null && identity.FindFirst(Constants.Claims.IdentityProvider) == null &&
            identity.FindFirst(OpenIddictConstants.Claims.AuthenticationMethodReference) == null)
        {
            // Удаляем оригинальный claim метода аутентификации
            identity.RemoveClaim(amr);

            // Добавляем claim идентификатора провайдера со значением из оригинального claim
            identity.AddClaim(new Claim(Constants.Claims.IdentityProvider, amr.Value));

            // Добавляем claim ссылки на метод аутентификации с внешним методом
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference,
                Constants.AuthenticationMethods.External));
        }

        // Гарантируем наличие claim идентификатора провайдера
        // Если claim отсутствует, добавляем локального провайдера
        if (identity.FindFirst(Constants.Claims.IdentityProvider) == null)
        {
            identity.AddClaim(new Claim(Constants.Claims.IdentityProvider, Constants.IdentityProviders.Local));
        }

        // Гарантируем наличие claim ссылки на метод аутентификации (amr)
        if (identity.FindFirst(OpenIddictConstants.Claims.AuthenticationMethodReference) == null)
        {
            // Если используется локальный провайдер, устанавливаем метод аутентификации по паролю
            if (identity.FindFirst(Constants.Claims.IdentityProvider)?.Value == Constants.IdentityProviders.Local)
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference,
                    Constants.AuthenticationMethods.Password));
            }
            // Для внешних провайдеров устанавливаем внешний метод аутентификации
            else
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference,
                    Constants.AuthenticationMethods.External));
            }
        }

        // Гарантируем наличие claim времени аутентификации (auth_time)
        // Этот claim требуется стандартом OpenID Connect
        if (identity.FindFirst(OpenIddictConstants.Claims.AuthenticationTime) == null)
        {
            // Преобразуем DateTime в Unix timestamp (секунды с 1970-01-01)
            var time = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds().ToString();

            // Добавляем claim с указанием типа значения - целое число 64 бита
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationTime, time,
                ClaimValueTypes.Integer64));
        }
    }
}